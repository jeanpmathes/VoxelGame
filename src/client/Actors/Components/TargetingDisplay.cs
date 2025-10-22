// <copyright file="TargetingDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Displays the information in <see cref="Targeting" /> in the world.
/// </summary>
public partial class TargetingDisplay : ActorComponent
{
    private readonly TargetingBoxEffect effect;
    private readonly Player player;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;

    [Constructible]
    private TargetingDisplay(Player player, Engine engine) : base(player)
    {
        this.player = player;

        effect = engine.TargetingBoxPipeline.CreateEffect();

        targeting = player.GetRequiredComponent<Targeting>();
    }

    /// <inheritdoc />
    public override void OnActivate()
    {
        SetTarget(world: null, state: null, position: null);
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        effect.IsEnabled = false;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        SetTarget(player.World, targeting.Block, targeting.Position);

        effect.LogicUpdate();
    }

    private void SetTarget(World? world, State? state, Vector3i? position)
    {
        BoxCollider? collider = null;

        if (world != null && state is {Block: {} block} && position != null)
        {
            Boolean visualized = !state.Value.IsReplaceable;

            if (Core.App.Application.Instance.IsDebug)
                visualized |= !block.IsEmpty;

            collider = visualized ? block.GetCollider(world, position.Value) : null;
        }

        if (collider != null)
            effect.SetBox(collider.Value);

        effect.IsEnabled = collider != null;
    }

    #region DISPOSABLE

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            effect.Dispose();
        }
    }

    #endregion DISPOSABLE
}
