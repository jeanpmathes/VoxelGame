// <copyright file="ITargetMeshData.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Interface that is fed with mesh data.
///     The used rendering platform can implement this to create meshes in the format it needs.
/// </summary>
public interface IMeshing : IDisposable
{
    /// <summary>
    ///     All possible primitives. The implementation can decide which ones to Graphics.
    ///     Other primitives can either be ignored or converted to supported primitives.
    /// </summary>
    enum Primitive
    {
        /// <summary>
        ///     A quad, which is a rectangle with two triangles.
        /// </summary>
        Quad
    }

    /// <summary>
    ///     Get the number of elements in the mesh.
    ///     As the used elements are implementation specific, different implementations can return different values.
    ///     If the mesh is empty, this should always return 0.
    /// </summary>
    Int32 Count { get; }

    /// <summary>
    ///     Push a quad to the mesh, while applying modifications to the positions and data.
    /// </summary>
    /// <param name="positions">The four positions of the quad, in clockwise order.</param>
    /// <param name="data">The data of the quad.</param>
    /// <param name="offset">The offset to apply to the positions.</param>
    void PushQuadWithOffset(
        in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions,
        in (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data,
        Vector3 offset);

    /// <summary>
    ///     Push a quad to the mesh.
    /// </summary>
    /// <param name="positions">The four positions of the quad, in clockwise order.</param>
    /// <param name="data">The data of the quad.</param>
    void PushQuad(
        in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions,
        in (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data);

    /// <summary>
    ///     Grow the internal structures to support the given number of additional primitives.
    ///     This is a hint and the implementation can ignore it.
    /// </summary>
    /// <param name="primitive">The primitive to grow for.</param>
    /// <param name="count">The number of primitives to grow for.</param>
    void Grow(Primitive primitive, Int32 count);
}
