// <copyright file="Slowing.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;

/// <summary>
///     Slows down all entities that come into contact with the block.
/// </summary>
public partial class Slowing : BlockBehavior, IBehavior<Slowing, BlockBehavior, Block>
{
    [Constructible]
    private Slowing(Block subject) : base(subject) {}

    /// <summary>
    ///     The maximum velocity that entities can have when in contact with this block.
    /// </summary>
    public ResolvedProperty<Double> MaxVelocity { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(MaxVelocity), initial: 1.0);

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorCollisionMessage>(OnActorCollision);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        MaxVelocity.Initialize(this);
    }

    private void OnActorCollision(Block.IActorCollisionMessage message)
    {
        var factor = 1.0;

        if (Subject.Get<PartialHeight>() is {} height) factor = height.GetHeight(message.State).Ratio;

        Vector3d newVelocity = MathTools.Clamp(message.Body.Velocity, min: -1.0, MaxVelocity.Get());

        message.Body.Velocity = Vector3d.Lerp(message.Body.Velocity, newVelocity, factor);
    }
}
