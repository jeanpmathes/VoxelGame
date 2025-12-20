// <copyright file="SpatialMeshing.cs" company="VoxelGame">
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
using VoxelGame.Core.Collections;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Data;

/// <summary>
///     Builds a mesh for <see cref="VoxelGame.Graphics.Objects.Mesh" />.
/// </summary>
public sealed class SpatialMeshing : IMeshing
{
    private readonly PooledList<SpatialVertex> mesh;

    /// <summary>
    ///     Creates a new meshing instance.
    /// </summary>
    /// <param name="sizeHint">A hint for the expected size of the mesh.</param>
    public SpatialMeshing(Int32 sizeHint)
    {
        mesh = new PooledList<SpatialVertex>(sizeHint);
    }

    /// <summary>
    ///     Get the mesh as a span.
    /// </summary>
    public Span<SpatialVertex> Span => mesh.AsSpan();

    /// <inheritdoc />
    public void PushQuadWithOffset(
        in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions,
        in (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data,
        Vector3 offset)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        mesh.Add(new SpatialVertex
        {
            Position = positions.a + offset,
            Data = data.a
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.b + offset,
            Data = data.b
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.c + offset,
            Data = data.c
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.d + offset,
            Data = data.d
        });
    }

    /// <inheritdoc />
    public void PushQuad(in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, in (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        mesh.Add(new SpatialVertex
        {
            Position = positions.a,
            Data = data.a
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.b,
            Data = data.b
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.c,
            Data = data.c
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.d,
            Data = data.d
        });
    }

    /// <inheritdoc />
    public void Grow(IMeshing.Primitive primitive, Int32 count)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        Int32 size = primitive == IMeshing.Primitive.Quad
            ? 4
            : throw Exceptions.UnsupportedEnumValue(primitive);

        mesh.EnsureCapacity(mesh.Count + size * count);
    }

    /// <inheritdoc />
    public Int32 Count => mesh.Count;

    #region DISPOSABLE

    private Boolean disposed;

    #pragma warning disable S2953 // False positive, this class does implement IDisposable.
    private void Dispose(Boolean disposing)
    #pragma warning restore S2953
    {
        if (disposed) return;

        if (disposing) mesh.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~SpatialMeshing()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
