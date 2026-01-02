// <copyright file="TargetedSidePlacement.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Places sided blocks based on the targeted side of the placing actor.
/// </summary>
public partial class TargetedSidePlacement : BlockBehavior, IBehavior<TargetedSidePlacement, BlockBehavior, Block>
{
    private readonly Sided siding;

    [Constructible]
    private TargetedSidePlacement(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        Side? side = actor?.GetTargetedSide()?.Opposite();

        if (side == null) return original;

        return siding.SetSides(original, side.Value.ToFlag()) ?? original;
    }
}
