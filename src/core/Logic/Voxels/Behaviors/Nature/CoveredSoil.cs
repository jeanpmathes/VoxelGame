// <copyright file="CoveredSoil.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Blocks with this behavior are soil blocks that are covered with something.
///     Some conditions like placement of a block on top remove the covering, turning the block into a regular soil block.
/// </summary>
public partial class CoveredSoil : BlockBehavior, IBehavior<CoveredSoil, BlockBehavior, Block>
{
    [Constructible]
    private CoveredSoil(Block subject) : base(subject)
    {
        subject.Require<Soil>();

        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IPlacementMessage>(OnPlacement);
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;

        return CanHaveCover(world, position) == false || Blocks.Instance.Environment.Soil.CanPlace(world, position, actor);
    }

    private static void OnPlacement(Block.IPlacementMessage message)
    {
        if (CanHaveCover(message.World, message.Position) == true)
            message.World.SetBlock(message.PlacementState, message.Position);
        else
            Blocks.Instance.Environment.Soil.Place(message.World, message.Position, message.Actor);
    }

    private static void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (message.Side == Side.Top && CanHaveCover(message.World, message.Position) != true)
            RemoveCover(message.World, message.Position);
    }

    /// <summary>
    ///     Check if a position can have cover on it.
    /// </summary>
    /// <param name="world">The world in which the position is located.</param>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if the position can have cover, <c>false</c> if it cannot, and <c>null</c> if it is unknown.</returns>
    public static Boolean? CanHaveCover(World world, Vector3i position)
    {
        State? top = world.GetBlock(position.Above());

        if (top == null)
            return null;

        State state = top.Value;

        if (state.Block.Get<CoverPreserving>() is {} coverPreserving)
            return coverPreserving.IsPreserving(state);

        if (state.IsSideFull(Side.Bottom))
            return false;

        return state.Block is not {IsOpaque: true, IsSolid: true};
    }

    /// <summary>
    ///     Remove the cover from a covered soil block, turning it into a regular soil block.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block.</param>
    public static void RemoveCover(World world, Vector3i position)
    {
        world.SetBlock(new State(Blocks.Instance.Environment.Soil), position);
    }
}
