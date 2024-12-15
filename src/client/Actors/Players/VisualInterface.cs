// <copyright file="PlayerVisualization.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Client.Application.Resources;
using VoxelGame.Client.Inputs;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Offers visualization like HUD, UI and in-world selection for the player.
/// </summary>
public sealed class VisualInterface : IDisposable
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly SelectionBoxVFX selectionVFX;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly OverlayVFX overlayVFX;

    private readonly List<VFX> vfxes = [];

    private readonly Button debugViewButton;

    private readonly GameUserInterface ui;

    /// <summary>
    ///     Create a new instance of the <see cref="VisualInterface" /> class.
    /// </summary>
    /// <param name="ui">The ui to use for some of the data display.</param>
    /// <param name="resources">The resources to use.</param>
    public VisualInterface(GameUserInterface ui, GameResources resources)
    {
        selectionVFX = RegisterVFX(resources.Pipelines.SelectionBoxVFX);
        overlayVFX = RegisterVFX(resources.Pipelines.OverlayVFX);

        ScreenElementVFX crosshairVFX = RegisterVFX(resources.Pipelines.CrosshairVFX);

        crosshairVFX.SetTexture(resources.Player.Crosshair);

        foreach (VFX renderer in vfxes) renderer.SetUp();

        KeybindManager keybind = Application.Client.Instance.Keybinds;
        debugViewButton = keybind.GetPushButton(keybind.DebugView);

        this.ui = ui;
    }

    /// <summary>
    ///     Whether overlay rendering is allowed. Building overlay is still possible, but it will not be rendered.
    /// </summary>
    public Boolean IsOverlayAllowed { get; set; } = true;

    private T RegisterVFX<T>(T vfx) where T : VFX
    {
        vfxes.Add(vfx);

        return vfx;
    }

    /// <summary>
    ///     Set the target block of the selection box.
    /// </summary>
    /// <param name="world">The world in which the block is, or null to disable the selection box.</param>
    /// <param name="instance">The block instance to target, or null to disable the selection box.</param>
    /// <param name="position">The position of the block, or null to disable the selection box.</param>
    public void SetSelectionBoxTarget(World? world, BlockInstance? instance, Vector3i? position)
    {
        Throw.IfDisposed(disposed);

        BoxCollider? collider = null;

        if (world != null && instance is {Block: {} block} && position != null)
        {
            Boolean visualized = !block.IsReplaceable;

            if (Program.IsDebug)
                visualized |= block != Blocks.Instance.Air;

            collider = visualized ? block.GetCollider(world, position.Value) : null;
        }

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

        SetSelectionBoxTarget(world: null, instance: null, position: null);

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
    /// <param name="player">The player for which to build the overlay.</param>
    /// <param name="positions">The positions to consider.</param>
    public void BuildOverlay(Player player, IEnumerable<(Content content, Vector3i position)> positions)
    {
        Throw.IfDisposed(disposed);

        overlayVFX.IsEnabled = false;
        var lowerBound = 1.0;
        var upperBound = 0.0;

        IEnumerable<Overlay> overlays = Overlay.MeasureOverlays(positions, player.View, ref lowerBound, ref upperBound).ToList();

        Overlay? selected = null;

        if (overlays.Any())
        {
            selected = overlays
                .OrderByDescending(o => o.Size)
                .ThenBy(o => (o.Position - player.Position).Length)
                .First();

            if (selected.IsBlock) overlayVFX.SetBlockTexture(selected.GetWithAppliedTint(player.World));
            else overlayVFX.SetFluidTexture(selected.GetWithAppliedTint(player.World));

            overlayVFX.IsEnabled = IsOverlayAllowed;
            overlayVFX.SetBounds(lowerBound, upperBound);
        }

        var size = 0.0;
        Color4? fog = selected?.GetFogColor(player.World);

        if (fog != null)
        {
            size = Math.Abs(upperBound - lowerBound);

            if (VMath.NearlyEqual(upperBound, b: 1.0) && lowerBound > 0.0)
                size *= -1.0;
        }

        Visuals.Graphics.Instance.SetFogOverlapConfiguration(size, fog ?? Color4.Black);
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

    #region DISPOSABLE

    private Boolean disposed;

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

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            foreach (VFX renderer in vfxes) renderer.TearDown();

            ui.Dispose();
        }

        disposed = true;
    }

    #endregion DISPOSABLE
}
