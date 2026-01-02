// <copyright file="StraightPipe.cs" company="VoxelGame">
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
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     A variant of a <see cref="Pipe" /> that only connects to other pipes in a straight line.
/// </summary>
public partial class StraightPipe : BlockBehavior, IBehavior<StraightPipe, BlockBehavior, Block>
{
    private readonly Piped piped;
    private readonly AxisRotatable rotation;

    [Constructible]
    private StraightPipe(Block subject) : base(subject)
    {
        rotation = subject.Require<AxisRotatable>();
        piped = subject.Require<Piped>();

        piped.IsConnectionAllowed.ContributeFunction(GetIsConnectionAllowed);

        subject.Require<Pipe>().OpenSides.ContributeFunction(GetOpenSides);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private Boolean GetIsConnectionAllowed(Boolean original, (State state, Side side) context)
    {
        (State state, Side side) = context;

        return rotation.GetAxis(state) == side.Axis();
    }

    private Sides GetOpenSides(Sides original, State state)
    {
        return rotation.GetAxis(state).Sides();
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Double diameter = Piped.GetPipeDiameter(piped.Tier.Get());

        Axis axis = rotation.GetAxis(state);

        return new BoundingVolume(new Vector3d(x: 0.5, y: 0.5, z: 0.5), axis.Vector3(onAxis: 0.5, diameter));
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        return rotation.SetAxis(original, (actor?.GetTargetedSide() ?? Side.Front).Axis());
    }
}
