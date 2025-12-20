// <copyright file="FluidMeshInfo.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Information required to mesh a fluid.
/// </summary>
public readonly record struct FluidMeshInfo
{
    private FluidMeshInfo(State block, FluidLevel level, Side side, Boolean isStatic)
    {
        Block = block;
        Level = level;
        Side = side;
        IsStatic = isStatic;
    }

    /// <summary>
    ///     Get the current block.
    /// </summary>
    public State Block { get; init; }

    /// <summary>
    ///     The level of the fluid.
    /// </summary>
    public FluidLevel Level { get; init; }

    /// <summary>
    ///     The side of the fluid that is being meshed.
    /// </summary>
    public Side Side { get; init; }

    /// <summary>
    ///     Whether the fluid is static.
    /// </summary>
    public Boolean IsStatic { get; init; }

    /// <summary>
    ///     Create fluid meshing information.
    /// </summary>
    public static FluidMeshInfo Fluid(State block, FluidLevel level, Side side, Boolean isStatic)
    {
        return new FluidMeshInfo(block, level, side, isStatic);
    }
}
