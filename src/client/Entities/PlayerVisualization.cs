// <copyright file="PlayerVisualization.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Support.Input.Actions;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities;

/// <summary>
///     Offers visualization like HUD, UI and in-world selection for the player.
/// </summary>
public sealed class PlayerVisualization : IDisposable
{
    private readonly Vector2 crosshairPosition = new(x: 0.5f, y: 0.5f);
    private readonly ScreenElementRenderer crosshairRenderer;
    private readonly Button debugViewButton;
    private readonly OverlayRenderer overlay;

    private readonly Player player;
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
    /// <param name="resources">The resources to use.</param>
    public PlayerVisualization(Player player, GameUserInterface ui, PlayerResources resources)
    {
        overlay = new OverlayRenderer();

        crosshairRenderer = new ScreenElementRenderer();
        crosshairRenderer.SetTexture(resources.Crosshair);
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

        IEnumerable<Overlay> overlays = Overlay.MeasureOverlays(positions, player, ref lowerBound, ref upperBound).ToList();

        if (!overlays.Any()) return;

        Overlay selected = overlays.OrderByDescending(o => o.Size).ThenBy(o => (o.Position - player.Position).Length).First();

        if (selected.IsBlock) overlay.SetBlockTexture(selected.GetWithAppliedTint(player.World));
        else overlay.SetFluidTexture(selected.GetWithAppliedTint(player.World));

        renderOverlay = true;

        FinalizeOverlay();
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

            overlay.Dispose();
            ui.Dispose();

            Application.Client.Instance.Settings.CrosshairColorChanged -= UpdateCrosshairColor;
        }

        disposed = true;
    }

    #endregion IDisposable Support
}


