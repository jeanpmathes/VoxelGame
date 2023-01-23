// <copyright file="PlayerVisualization.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Objects;
using VoxelGame.Input.Actions;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities;

/// <summary>
///     Offers visualization like HUD, UI and in-world selection for the player.
/// </summary>
public sealed class PlayerVisualization : IDisposable
{
    private readonly Texture crosshair;
    private readonly Vector2 crosshairPosition = new(x: 0.5f, y: 0.5f);
    private readonly ScreenElementRenderer crosshairRenderer;
    private readonly Button debugViewButton;
    private readonly OverlayRenderer overlay;

    private readonly ClientPlayer player;
    private readonly BoxRenderer selectionRenderer;
    private readonly GameUserInterface ui;
    private float crosshairScale = Application.Client.Instance.Settings.CrosshairScale;

    private double lowerBound;
    private bool renderOverlay;
    private double upperBound;

    /// <summary>
    ///     Create a new instance of the <see cref="PlayerVisualization" /> class.
    /// </summary>
    /// <param name="player">The player that is visualized.</param>
    /// <param name="ui">The ui to use for some of the data display.</param>
    public PlayerVisualization(ClientPlayer player, GameUserInterface ui)
    {
        overlay = new OverlayRenderer();

        crosshair = new Texture(
            "Resources/Textures/UI/crosshair.png",
            TextureUnit.Texture10,
            fallbackResolution: 32);

        crosshairRenderer = new ScreenElementRenderer();
        crosshairRenderer.SetTexture(crosshair);
        crosshairRenderer.SetColor(Application.Client.Instance.Settings.CrosshairColor);

        Application.Client.Instance.Settings.CrosshairColorChanged += UpdateCrosshairColor;
        Application.Client.Instance.Settings.CrosshairScaleChanged += SettingsOnCrosshairScaleChanged;

        selectionRenderer = new BoxRenderer();

        KeybindManager keybind = Application.Client.Instance.Keybinds;
        debugViewButton = keybind.GetPushButton(keybind.DebugView);

        ClearOverlay();

        this.ui = ui;
        this.player = player;
    }

    private void UpdateCrosshairColor(object? sender, SettingChangedArgs<Color> args)
    {
        crosshairRenderer.SetColor(args.Settings.CrosshairColor);
    }

    private void SettingsOnCrosshairScaleChanged(object? sender, SettingChangedArgs<float> args)
    {
        crosshairScale = args.NewValue;
    }

    /// <summary>
    ///     Draw the normal visualization elements.
    /// </summary>
    public void Draw()
    {
        crosshairRenderer.Draw(crosshairPosition, crosshairScale);
    }

    /// <summary>
    ///     Draw a selection box.
    /// </summary>
    /// <param name="collider">The collider to draw.</param>
    public void DrawSelectionBox(BoxCollider collider)
    {
        selectionRenderer.SetVolume(collider.Volume);
        selectionRenderer.Draw(collider.Position);
    }

    /// <summary>
    ///     Draw an overlay, used to indicate a substance surrounding the player head.
    /// </summary>
    public void DrawOverlay()
    {
        if (renderOverlay) overlay.Draw();
    }

    /// <summary>
    /// Build the overlay, considering the given positions.
    /// </summary>
    /// <param name="positions">The positions to consider.</param>
    public void BuildOverlay(IEnumerable<(Content content, Vector3i position)> positions)
    {
        ClearOverlay();

        IEnumerable<(double size, int index, bool isBlock)> overlays = MeasureOverlays(positions).ToList();

        if (!overlays.Any()) return;

        (double size, int index, bool isBlock) max = overlays.MaxBy(x => x.size);

        if (max.isBlock) overlay.SetBlockTexture(max.index);
        else overlay.SetFluidTexture(max.index);

        renderOverlay = true;

        FinalizeOverlay();
    }

    private IEnumerable<(double size, int index, bool isBlock)> MeasureOverlays(IEnumerable<(Content content, Vector3i position)> positions)
    {
        List<(double size, int index, bool isBlock)> overlays = new();

        var anyIsBlock = false;

        foreach ((Content content, Vector3i position) in positions)
        {
            (double, double)? newBounds = null;
            IOverlayTextureProvider? overlayTextureProvider = null;
            var isBlock = false;

            if (content.Block.Block is IOverlayTextureProvider overlayBlockTextureProvider)
            {
                newBounds = GetOverlayBounds(content.Block, position);
                overlayTextureProvider = overlayBlockTextureProvider;

                isBlock = true;
                anyIsBlock = true;
            }

            if (newBounds == null && content.Fluid.Fluid is IOverlayTextureProvider overlayFluidTextureProvider)
            {
                newBounds = GetOverlayBounds(content.Fluid, position);
                overlayTextureProvider = overlayFluidTextureProvider;
            }

            if (newBounds is null) continue;

            (double newLowerBound, double newUpperBound) = newBounds.Value;
            int textureIndex = overlayTextureProvider!.TextureIdentifier;

            lowerBound = Math.Min(newLowerBound, lowerBound);
            upperBound = Math.Max(newUpperBound, upperBound);

            overlays.Add((newUpperBound - newLowerBound, textureIndex, isBlock));
        }

        return anyIsBlock ? overlays.Where(x => x.isBlock) : overlays;
    }

    private (double lower, double upper)? GetOverlayBounds(BlockInstance block, Vector3d position)
    {
        var height = 15;

        if (block.Block is IHeightVariable heightVariable) height = heightVariable.GetHeight(block.Data);

        return GetOverlayBounds(height, position, inverted: false);
    }

    private (double lower, double upper)? GetOverlayBounds(FluidInstance fluid, Vector3d position)
    {
        int height = fluid.Level.GetBlockHeight();

        return GetOverlayBounds(height, position, fluid.Fluid.Direction == VerticalFlow.Upwards);
    }

    private (double lower, double upper)? GetOverlayBounds(int height, Vector3d position, bool inverted)
    {
        float actualHeight = (height + 1) * (1.0f / 16.0f);
        if (inverted) actualHeight = 1.0f - actualHeight;

        Plane topPlane = new(Vector3d.UnitY, position + Vector3d.UnitY * actualHeight);
        Plane viewPlane = player.View.Frustum.Near;

        Line? bound = topPlane.Intersects(viewPlane);

        if (bound == null) return null;

        Vector3d axis = player.Right;
        (Vector3d a, Vector3d b) dimensions = player.NearDimensions;

        // Assume the bound is parallel to the view horizon.
        Vector2d point = viewPlane.Project2D(bound.Value.Any, axis);
        Vector2d a = viewPlane.Project2D(dimensions.a, axis);
        Vector2d b = viewPlane.Project2D(dimensions.b, axis);

        double ratio = VMath.InverseLerp(a.Y, b.Y, point.Y);

        (double newLowerBound, double newUpperBound) = inverted ? (ratio, 1.0) : (0.0, ratio);

        newLowerBound = Math.Max(newLowerBound, val2: 0);
        newUpperBound = Math.Min(newUpperBound, val2: 1);

        return (newLowerBound, newUpperBound);
    }

    private void FinalizeOverlay()
    {
        overlay.SetBounds(lowerBound, upperBound);
    }

    private void ClearOverlay()
    {
        renderOverlay = false;

        lowerBound = 1.0f;
        upperBound = 0.0f;
    }

    /// <summary>
    ///     Perform updates depending on user input.
    /// </summary>
    public void UpdateInput()
    {
        SelectDebugView();
    }

    private void SelectDebugView()
    {
        if (debugViewButton.IsDown) ui.ToggleDebugDataView();
    }

    /// <summary>
    ///     Perform normal updates.
    /// </summary>
    public void Update()
    {
        ui.UpdatePlayerDebugData();
    }

    /// <summary>
    ///     Update the player data display.
    /// </summary>
    public void UpdateData()
    {
        ui.UpdatePlayerData();
    }

    #region IDisposable Support

    private bool disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~PlayerVisualization()
    {
        Dispose(disposing: false);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            crosshairRenderer.Dispose();
            selectionRenderer.Dispose();
            crosshair.Dispose();
            overlay.Dispose();
            ui.Dispose();

            Application.Client.Instance.Settings.CrosshairColorChanged -= UpdateCrosshairColor;
        }

        disposed = true;
    }

    #endregion IDisposable Support
}
