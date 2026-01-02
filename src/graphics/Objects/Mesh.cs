// <copyright file="Mesh.cs" company="VoxelGame">
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
using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A mesh, positioned in 3D space and target of raytracing.
/// </summary>
[NativeMarshalling(typeof(MeshMarshaller))]
public class Mesh : Drawable
{
    /// <summary>
    ///     Wrap a native mesh and drawable pointer.
    /// </summary>
    public Mesh(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the vertices that define this mesh.
    ///     Only valid if the material uses the default intersection shader.
    /// </summary>
    /// <param name="vertices">The vertices.</param>
    public void SetVertices(Span<SpatialVertex> vertices)
    {
        Native.SetMeshVertices(this, vertices);
    }

    /// <summary>
    ///     Set the bounds that define this mesh.
    ///     Only valid if the material uses a custom intersection shader.
    /// </summary>
    /// <param name="bounds">The bounds.</param>
    public void SetBounds(Span<SpatialBounds> bounds)
    {
        Native.SetMeshBounds(this, bounds);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Mesh), MarshalMode.ManagedToUnmanagedIn, typeof(MeshMarshaller))]
internal static class MeshMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Mesh managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
