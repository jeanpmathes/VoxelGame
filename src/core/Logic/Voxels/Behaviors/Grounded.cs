// <copyright file="Grounded.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     Makes a block require fully solid ground to be placed.
/// </summary>
public partial class Grounded : BlockBehavior, IBehavior<Grounded, BlockBehavior, Block>
{
    private Boolean isComposite;

    [Constructible]
    private Grounded(Block subject) : base(subject)
    {
        subject.RequireIfPresent<CompositeGrounded, Composite>(_ => isComposite = true);

        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        if (isComposite) return;

        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    private Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        if (isComposite) return true;

        (World world, Vector3i position, Actor? _) = context;

        return IsGrounded(world, position);
    }

    private static void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        Vector3i positionBelow = message.Position.Below();
        State blockBelow = message.World.GetBlock(positionBelow) ?? Content.DefaultState;

        if (blockBelow.IsFullySolid)
            return;

        if (blockBelow.Block.Get<CompletableGround>() is {} completableGround)
            completableGround.BecomeComplete(message.World, positionBelow);
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (message.Side != Side.Bottom || IsGrounded(message.World, message.Position)) return;

        Subject.ScheduleDestroy(message.World, message.Position);
    }

    /// <summary>
    ///     Check if a position is grounded.
    /// </summary>
    internal static Boolean IsGrounded(World world, Vector3i position)
    {
        Vector3i positionBelow = position.Below();
        State blockBelow = world.GetBlock(positionBelow) ?? Content.DefaultState;

        return blockBelow.IsFullySolid || blockBelow.Block.Is<CompletableGround>();
    }
}
