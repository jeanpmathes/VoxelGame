// <copyright file="Cactus.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     A simple cactus.
/// </summary>
public class Cactus : DynamicStructure
{
    /// <inheritdoc />
    public override Vector3i Extents => new(x: 1, y: 3, z: 1);

    /// <inheritdoc />
    protected override Random? GetRandomness(Int32 seed)
    {
        return null;
    }

    /// <inheritdoc />
    protected override (Content content, Boolean overwrite)? GetContent(Vector3i offset, Single random)
    {
        return (Content.CreateGenerated(Blocks.Instance.Organic.Cactus), true);
    }
}
