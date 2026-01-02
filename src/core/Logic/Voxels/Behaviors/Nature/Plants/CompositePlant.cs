// <copyright file="CompositePlant.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     Glue behavior for plants that are made up of multiple parts.
/// </summary>
public partial class CompositePlant : BlockBehavior, IBehavior<CompositePlant, BlockBehavior, Block>
{
    [Constructible]
    private CompositePlant(Block subject) : base(subject)
    {
        subject.Require<Composite>().IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Composite.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Vector3i part, Actor? actor) context)
    {
        (World world, Vector3i position, Vector3i part, Actor? _) = context;

        if (part.Y != 0) return true;

        State? ground = world.GetBlock(position.Below());

        return ground?.Block.Is<Plantable>() == true;
    }

    private void OnNeighborUpdate(Composite.INeighborUpdateMessage message)
    {
        if (message.Part.Y != 0)
            return;

        if (message.Side != Side.Bottom)
            return;

        if (message.World.GetBlock(message.Position.Below())?.Block.Is<Plantable>() != true)
            Subject.Destroy(message.World, message.Position);
    }
}
