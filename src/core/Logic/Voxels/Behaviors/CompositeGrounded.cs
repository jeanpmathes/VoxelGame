// <copyright file="CompositeGrounded.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     Adapts the Grounded behavior for composite blocks.
/// </summary>
public partial class CompositeGrounded : BlockBehavior, IBehavior<CompositeGrounded, BlockBehavior, Block>
{
    [Constructible]
    private CompositeGrounded(Block subject) : base(subject)
    {
        subject.Require<Composite>().IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Composite.IPlacementCompletedMessage>(OnPlacementCompleted);
        bus.Subscribe<Composite.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Vector3i part, Actor? actor) context)
    {
        (World world, Vector3i position, Vector3i part, Actor? _) = context;

        return part.Y != 0 || Grounded.IsGrounded(world, position);
    }

    private static void OnPlacementCompleted(Composite.IPlacementCompletedMessage message)
    {
        Vector3i positionBelow = message.Position.Below();
        State blockBelow = message.World.GetBlock(positionBelow) ?? Content.DefaultState;

        if (blockBelow.IsFullySolid)
            return;

        if (blockBelow.Block.Get<CompletableGround>() is {} completableGround)
            completableGround.BecomeComplete(message.World, positionBelow);
    }

    private void OnNeighborUpdate(Composite.INeighborUpdateMessage message)
    {
        if (message.Part.Y != 0 || message.Side != Side.Bottom) return;

        if (!Grounded.IsGrounded(message.World, message.Position))
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
