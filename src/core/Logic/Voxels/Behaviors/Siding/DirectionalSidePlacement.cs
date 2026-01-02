// <copyright file="DirectionalSidePlacement.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Places sided blocks based on the direction the placing actor is facing.
/// </summary>
public partial class DirectionalSidePlacement : BlockBehavior, IBehavior<DirectionalSidePlacement, BlockBehavior, Block>
{
    private readonly Sided siding;

    [Constructible]
    private DirectionalSidePlacement(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        var orientation = actor?.Head?.Forward.ToOrientation();

        if (orientation == null) return original;

        return siding.SetSides(original, orientation.Value.ToSide().Opposite().ToFlag()) ?? original;
    }
}
