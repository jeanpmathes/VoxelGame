// <copyright file="FluidMeshData.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Data for meshing fluids.
/// </summary>
public sealed class FluidMeshData
{
    private FluidMeshData(Int32 textureIndex, ColorS tint)
    {
        TextureIndex = textureIndex;
        Tint = tint;
    }

    /// <summary>
    ///     The texture index.
    /// </summary>
    public Int32 TextureIndex { get; }

    /// <summary>
    ///     The tint color.
    /// </summary>
    public ColorS Tint { get; }

    /// <summary>
    ///     Creates fluid mesh data for an empty fluid.
    /// </summary>
    public static FluidMeshData Empty { get; } = new(textureIndex: 0, ColorS.NoTint);

    /// <summary>
    ///     Creates fluid mesh data for a basic fluid.
    /// </summary>
    /// <param name="textureIndex">The texture index.</param>
    /// <param name="tint">The tint color.</param>
    /// <returns>The mesh data.</returns>
    public static FluidMeshData Basic(Int32 textureIndex, ColorS tint)
    {
        return new FluidMeshData(textureIndex, tint);
    }
}
