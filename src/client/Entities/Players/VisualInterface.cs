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

namespace VoxelGame.Client.Entities.Players;

/// <summary>
///     Offers visualization like HUD, UI and in-world selection for the player.
/// </summary>
public sealed class VisualInterface : IDisposable
{
    private readonly ScreenElementRenderer crosshairRenderer;
    private readonly OverlayRenderer overlay;
    private readonly List<Renderer> renderers = new();

    private readonly Button debugViewButton;

    private readonly Player player;
    private readonly SelectionBoxRenderer selectionRenderer;
    private readonly GameUserInterface ui;

    private double lowerBound;
    private bool renderOverlay;
    private double upperBound;

    /// <summary>
    ///     Create a new instance of the <see cref="VisualInterface" /> class.
    /// </summary>
    /// <param name="player">The player that is visualized.</param>
    /// <param name="ui">The ui to use for some of the data display.</param>
    /// <param name="resources">The resources to use.</param>
    public VisualInterface(Player player, GameUserInterface ui, PlayerResources resources)
    {
        overlay = new OverlayRenderer();

        selectionRenderer = RegisterRenderer(Application.Client.Instance.Resources.Pipelines.SelectionBoxRenderer);
        crosshairRenderer = RegisterRenderer(Application.Client.Instance.Resources.Pipelines.CrosshairRenderer);

        foreach (Renderer renderer in renderers) renderer.SetUp();

        {
            // todo: all settings sync should move to pipelines class, and use new Binding<T> utility class (which could be declared directly in Settings, grouping value and delegate, providing IDisposable to unbind

            crosshairRenderer.SetTexture(resources.Crosshair);
            crosshairRenderer.SetColor(Application.Client.Instance.Settings.CrosshairColor);
            crosshairRenderer.SetScale(Application.Client.Instance.Settings.CrosshairScale);

            Application.Client.Instance.Settings.CrosshairColorChanged += UpdateCrosshairColor;
            Application.Client.Instance.Settings.CrosshairScaleChanged += UpdateCrosshairScale;
        }

        KeybindManager keybind = Application.Client.Instance.Keybinds;
        debugViewButton = keybind.GetPushButton(keybind.DebugView);

        ClearOverlay();

        this.ui = ui;
        this.player = player;
    }

    private T RegisterRenderer<T>(T renderer) where T : Renderer
    {
        renderers.Add(renderer);

        return renderer;
    }

    private void UpdateCrosshairColor(object? sender, SettingChangedArgs<Color> args)
    {
        crosshairRenderer.SetColor(args.NewValue);
    }

    private void UpdateCrosshairScale(object? sender, SettingChangedArgs<float> args)
    {
        crosshairRenderer.SetScale(args.NewValue);
    }

    /// <summary>
    ///     Set the selection box which is drawn in the world.
    /// </summary>
    /// <param name="collider">The collider to draw, or null to not draw anything.</param>
    public void SetSelectionBox(BoxCollider? collider)
    {
        if (collider != null)
            selectionRenderer.SetBox(collider.Value);

        selectionRenderer.IsEnabled = collider != null;
    }

    /// <summary>
    ///     Call this when the player is activated.
    /// </summary>
    public void Activate()
    {
        foreach (Renderer renderer in renderers) renderer.IsEnabled = true;

        SetSelectionBox(collider: null);
    }

    /// <summary>
    ///     Call this when the player is deactivated.
    /// </summary>
    public void Deactivate()
    {
        foreach (Renderer renderer in renderers) renderer.IsEnabled = false;
    }

    /// <summary>
    ///     Draw an overlay, used to indicate a substance surrounding the player head.
    /// </summary>
    public void DrawOverlay() // todo: remove this method, move the renderOverlay bool to the renderer
    {
        if (renderOverlay) overlay.Draw();
    }

    /// <summary>
    ///     Build the overlay, considering the given positions.
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
        foreach (Renderer renderer in renderers) renderer.Update();

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
    ~VisualInterface()
    {
        Dispose(disposing: false);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            foreach (Renderer renderer in renderers) renderer.TearDown();

            overlay.Dispose(); // todo: move creation to pipelines class, dispose there

            ui.Dispose();

            Application.Client.Instance.Settings.CrosshairColorChanged -= UpdateCrosshairColor; // todo: move to the pipelines class (init too), simplify with Binding<T>
            Application.Client.Instance.Settings.CrosshairScaleChanged -= UpdateCrosshairScale;
        }

        disposed = true;
    }

    #endregion IDisposable Support
}
