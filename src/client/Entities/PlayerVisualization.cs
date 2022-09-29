// <copyright file="PlayerVisualization.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;
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
    ///     Add a potential overlay for one position.
    /// </summary>
    /// <param name="block">The block around the player head.</param>
    /// <param name="fluid">The fluid around the player head.</param>
    /// <param name="position">The position of the block/fluid around the player head.</param>
    public void AddOverlay(BlockInstance block, FluidInstance fluid, Vector3i position)
    {
        if (block.Block is IOverlayTextureProvider overlayBlockTextureProvider)
        {
            overlay.SetBlockTexture(overlayBlockTextureProvider.TextureIdentifier);
            SetOverlayBounds(block, position);

            renderOverlay = true;
        }
        else if (fluid.Fluid is IOverlayTextureProvider overlayFluidTextureProvider)
        {
            overlay.SetFluidTexture(overlayFluidTextureProvider.TextureIdentifier);
            SetOverlayBounds(fluid, position);

            renderOverlay = true;
        }
        else
        {
            renderOverlay = false;
        }
    }

    private void SetOverlayBounds(BlockInstance block, Vector3d position)
    {
        var height = 15;

        if (block.Block is IHeightVariable heightVariable) height = heightVariable.GetHeight(block.Data);

        SetOverlayBounds(height, position, inverted: false);
    }

    private void SetOverlayBounds(FluidInstance fluid, Vector3d position)
    {
        int height = fluid.Level.GetBlockHeight();

        SetOverlayBounds(height, position, fluid.Fluid.Direction == VerticalFlow.Upwards);
    }

    private void SetOverlayBounds(int height, Vector3d position, bool inverted)
    {
        float actualHeight = (height + 1) * (1.0f / 16.0f);
        if (inverted) actualHeight = 1.0f - actualHeight;

        Plane topPlane = new(Vector3d.UnitY, position + Vector3d.UnitY * actualHeight);
        Plane viewPlane = player.View.Frustum.Near;

        Line? bound = topPlane.Intersects(viewPlane);

        if (bound == null) return;

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

        lowerBound = Math.Min(newLowerBound, lowerBound);
        upperBound = Math.Max(newUpperBound, upperBound);
    }

    /// <summary>
    ///     Finalize the overlay after adding all elements.
    /// </summary>
    public void FinalizeOverlay()
    {
        overlay.SetBounds(lowerBound, upperBound);
    }

    /// <summary>
    ///     Clear the current overlay, if there is one.
    /// </summary>
    public void ClearOverlay()
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
