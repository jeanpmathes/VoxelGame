// <copyright file="PlayerVisualization.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Input.Actions;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities.Players;

/// <summary>
///     Offers visualization like HUD, UI and in-world selection for the player.
/// </summary>
public sealed class VisualInterface : IDisposable
{
    private readonly SelectionBoxVFX selectionVFX;
    private readonly OverlayVFX overlayVFX;
    private readonly List<VFX> vfxes = new();

    private readonly Button debugViewButton;

    private readonly Player player;

    private readonly GameUserInterface ui;

    /// <summary>
    ///     Create a new instance of the <see cref="VisualInterface" /> class.
    /// </summary>
    /// <param name="player">The player that is visualized.</param>
    /// <param name="ui">The ui to use for some of the data display.</param>
    /// <param name="resources">The resources to use.</param>
    public VisualInterface(Player player, GameUserInterface ui, GameResources resources)
    {
        selectionVFX = RegisterVFX(resources.Pipelines.SelectionBoxVFX);
        overlayVFX = RegisterVFX(resources.Pipelines.OverlayVFX);
        ScreenElementVFX crosshairVFX = RegisterVFX(resources.Pipelines.CrosshairVFX);

        crosshairVFX.SetTexture(resources.Player.Crosshair);

        foreach (VFX renderer in vfxes) renderer.SetUp();

        KeybindManager keybind = Application.Client.Instance.Keybinds;
        debugViewButton = keybind.GetPushButton(keybind.DebugView);

        this.ui = ui;
        this.player = player;
    }

    /// <summary>
    /// Whether overlay rendering is allowed. Building overlay is still possible, but it will not be rendered.
    /// </summary>
    public bool IsOverlayAllowed { get; set; } = true;

    private T RegisterVFX<T>(T vfx) where T : VFX
    {
        vfxes.Add(vfx);

        return vfx;
    }

    /// <summary>
    ///     Set the selection box which is drawn in the world.
    /// </summary>
    /// <param name="collider">The collider to draw, or null to not draw anything.</param>
    public void SetSelectionBox(BoxCollider? collider)
    {
        Throw.IfDisposed(disposed);

        if (collider != null)
            selectionVFX.SetBox(collider.Value);

        selectionVFX.IsEnabled = collider != null;
    }

    /// <summary>
    ///     Call this when the player is activated.
    /// </summary>
    public void Activate()
    {
        Throw.IfDisposed(disposed);

        foreach (VFX renderer in vfxes) renderer.IsEnabled = true;

        SetSelectionBox(collider: null);

        ui.SetActive(active: true);
    }

    /// <summary>
    ///     Call this when the player is deactivated.
    /// </summary>
    public void Deactivate()
    {
        Throw.IfDisposed(disposed);

        ui.SetActive(active: false);

        foreach (VFX renderer in vfxes) renderer.IsEnabled = false;
    }

    /// <summary>
    ///     Build the overlay, considering the given positions.
    /// </summary>
    /// <param name="positions">The positions to consider.</param>
    public void BuildOverlay(IEnumerable<(Content content, Vector3i position)> positions)
    {
        Throw.IfDisposed(disposed);

        overlayVFX.IsEnabled = false;
        var lowerBound = 1.0;
        var upperBound = 0.0;

        IEnumerable<Overlay> overlays = Overlay.MeasureOverlays(positions, player.View.Frustum, ref lowerBound, ref upperBound).ToList();

        if (!overlays.Any()) return;

        Overlay selected = overlays.OrderByDescending(o => o.Size).ThenBy(o => (o.Position - player.Position).Length).First();

        if (selected.IsBlock) overlayVFX.SetBlockTexture(selected.GetWithAppliedTint(player.World));
        else overlayVFX.SetFluidTexture(selected.GetWithAppliedTint(player.World));

        overlayVFX.IsEnabled = IsOverlayAllowed;
        overlayVFX.SetBounds(lowerBound, upperBound);
    }

    /// <summary>
    ///     Perform updates depending on user input.
    /// </summary>
    public void UpdateInput()
    {
        Throw.IfDisposed(disposed);

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
        Throw.IfDisposed(disposed);

        foreach (VFX renderer in vfxes) renderer.Update();

        ui.UpdatePlayerDebugData();
    }

    /// <summary>
    ///     Update the player data display.
    /// </summary>
    public void UpdateData()
    {
        Throw.IfDisposed(disposed);

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
            foreach (VFX renderer in vfxes) renderer.TearDown();

            ui.Dispose();
        }

        disposed = true;
    }

    #endregion IDisposable Support
}
