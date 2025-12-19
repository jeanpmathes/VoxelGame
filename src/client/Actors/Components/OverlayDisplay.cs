// <copyright file="OverlayDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Controls the block / fluid overlay for the player.
/// </summary>
public partial class OverlayDisplay : ActorComponent
{
    private readonly Engine engine;
    private readonly Player player;

    [Constructible]
    private OverlayDisplay(Player player, Engine engine) : base(player)
    {
        this.player = player;
        this.engine = engine;
    }

    /// <summary>
    ///     Whether overlay rendering (of block / fluid overlays) is allowed. Building overlay is still possible, but it will
    ///     not be rendered.
    /// </summary>
    public Boolean IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public override void OnActivate()
    {
        engine.OverlayPipeline.IsEnabled = true;
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        engine.OverlayPipeline.IsEnabled = false;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        Vector3i center = player.Camera.Transform.Position.Floor();
        Frustum frustum = player.Camera.View.GetPartialFrustum(near: 0.0, player.Camera.View.Definition.Clipping.near);

        BuildOverlay(Raycast.CastFrustum(player.World, center, range: 1, frustum));

        engine.OverlayPipeline.LogicUpdate();
    }

    private void BuildOverlay(IEnumerable<(Content content, Vector3i position)> positions)
    {
        engine.OverlayPipeline.IsEnabled = false;
        var lowerBound = 1.0;
        var upperBound = 0.0;

        IEnumerable<Overlay> overlays = Overlay.MeasureOverlays(positions, player.Camera, ref lowerBound, ref upperBound).ToList();

        Overlay? selected = null;

        if (overlays.Any())
        {
            selected = overlays
                .OrderByDescending(o => o.Size)
                .ThenBy(o => (o.Position - player.Body.Transform.Position).Length)
                .First();

            if (selected.IsBlock) engine.OverlayPipeline.SetBlockTexture(selected.GetWithAppliedTint(player.World));
            else engine.OverlayPipeline.SetFluidTexture(selected.GetWithAppliedTint(player.World));

            engine.OverlayPipeline.IsEnabled = IsEnabled;
            engine.OverlayPipeline.SetBounds(lowerBound, upperBound);
        }

        var size = 0.0;
        ColorS? fog = selected?.GetFogColor(player.World);

        if (fog != null)
        {
            size = Math.Abs(upperBound - lowerBound);

            if (MathTools.NearlyEqual(upperBound, b: 1.0) && lowerBound > 0.0)
                size *= -1.0;
        }

        Visuals.Graphics.Instance.SetFogVolumeOverlapConfiguration(size, fog ?? ColorS.Black);
    }
}
