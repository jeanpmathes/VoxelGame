// <copyright file="Wall.cs" company="VoxelGame">
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
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

/// <summary>
///     Provides the bounding volume for the <see cref="WideConnecting" /> wall block.
/// </summary>
public partial class Wall : BlockBehavior, IBehavior<Wall, BlockBehavior, Block>
{
    private readonly Connecting connecting;

    [Constructible]
    private Wall(Block subject) : base(subject)
    {
        subject.Require<WideConnecting>();
        connecting = subject.Require<Connecting>();

        subject.Require<Fillable>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);

        Boolean useStraightZ = north && south && !east && !west;
        Boolean useStraightX = !north && !south && east && west;

        if (useStraightZ)
            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.5f));

        if (useStraightX)
            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.1875f));

        List<BoundingVolume> children = new(capacity: 6);


        if (north)
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.125f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.125f)));

        if (east)
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.875f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.1875f)));

        if (south)
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.875f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.125f)));

        if (west)
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.1875f)));

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.25f, y: 0.5f, z: 0.25f),
            children.ToArray());
    }
}
