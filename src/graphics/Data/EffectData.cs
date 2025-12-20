// <copyright file="EffectData.cs" company="VoxelGame">
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
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Data;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Vertex type used by the <see cref="Effect" /> class.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EffectVertex
{
    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     Additional data for the vertex, for any purpose determined by the shader.
    /// </summary>
    public UInt32 Data;
}
