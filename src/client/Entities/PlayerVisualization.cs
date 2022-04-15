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
    private readonly BoxRenderer selectionRenderer;
    private readonly GameUserInterface ui;
    private float crosshairScale = Application.Client.Instance.Settings.CrosshairScale;
    private bool renderOverlay;

    /// <summary>
    ///     Create a new instance of the <see cref="PlayerVisualization" /> class.
    /// </summary>
    /// <param name="ui">The ui to use for some of the data display.</param>
    public PlayerVisualization(GameUserInterface ui)
    {
        overlay = new OverlayRenderer();

        crosshair = new Texture(
            "Resources/Textures/UI/crosshair.png",
            TextureUnit.Texture10,
            fallbackResolution: 32);

        crosshairRenderer = new ScreenElementRenderer();
        crosshairRenderer.SetTexture(crosshair);
        crosshairRenderer.SetColor(Application.Client.Instance.Settings.CrosshairColor.ToVector3());

        Application.Client.Instance.Settings.CrosshairColorChanged += UpdateCrosshairColor;
        Application.Client.Instance.Settings.CrosshairScaleChanged += SettingsOnCrosshairScaleChanged;

        selectionRenderer = new BoxRenderer();

        KeybindManager keybind = Application.Client.Instance.Keybinds;
        debugViewButton = keybind.GetPushButton(keybind.DebugView);

        this.ui = ui;
    }

    private void UpdateCrosshairColor(object? sender, SettingChangedArgs<Color> args)
    {
        crosshairRenderer.SetColor(args.Settings.CrosshairColor.ToVector3());
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
    ///     Set the overlay.
    /// </summary>
    /// <param name="block">The block around the player head.</param>
    /// <param name="fluid">The fluid around the player head.</param>
    public void SetOverlay(Block block, Fluid fluid)
    {
        if (block is IOverlayTextureProvider overlayBlockTextureProvider)
        {
            overlay.SetBlockTexture(overlayBlockTextureProvider.TextureIdentifier);
            renderOverlay = true;
        }
        else if (fluid is IOverlayTextureProvider overlayFluidTextureProvider)
        {
            overlay.SetFluidTexture(overlayFluidTextureProvider.TextureIdentifier);
            renderOverlay = true;
        }
        else
        {
            renderOverlay = false;
        }
    }

    /// <summary>
    ///     Clear the current overlay, if there is one.
    /// </summary>
    public void ClearOverlay()
    {
        renderOverlay = false;
    }

    /// <summary>
    ///     Checks whether either the given block or fluid provides an overlay texture.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <param name="fluid">The fluid.</param>
    /// <returns>True if either one provides a texture.</returns>
    public static bool CanSetOverlayFrom(Block block, Fluid fluid)
    {
        return block is IOverlayTextureProvider || fluid is IOverlayTextureProvider;
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
