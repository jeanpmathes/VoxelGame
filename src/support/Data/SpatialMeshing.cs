// <copyright file="SpatialMeshing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Support.Data;

/// <summary>
///     Builds a mesh for <see cref="VoxelGame.Support.Objects.Mesh" />.
/// </summary>
public sealed class SpatialMeshing : IMeshing
{
    private readonly PooledList<SpatialVertex> mesh;

    /// <summary>
    ///     Creates a new meshing instance.
    /// </summary>
    /// <param name="sizeHint">A hint for the expected size of the mesh.</param>
    public SpatialMeshing(int sizeHint)
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
        in (uint a, uint b, uint c, uint d) data,
        Vector3 offset)
    {
        Throw.IfDisposed(disposed);

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
    public void PushQuad(in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions, in (uint a, uint b, uint c, uint d) data)
    {
        Throw.IfDisposed(disposed);

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
    public void Grow(IMeshing.Primitive primitive, int count)
    {
        Throw.IfDisposed(disposed);

        int size = primitive == IMeshing.Primitive.Quad
            ? 4
            : throw new ArgumentOutOfRangeException(nameof(primitive), primitive, message: null);

        mesh.EnsureCapacity(mesh.Count + size * count);
    }

    /// <inheritdoc />
    public int Count => mesh.Count;

    #region IDisposable Support

    private bool disposed;

    #pragma warning disable S2953 // False positive, this class does implement IDisposable.
    private void Dispose(bool disposing)
    #pragma warning restore S2953
    {
        if (disposed) return;

        if (disposing) mesh.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
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

    #endregion IDisposable Support
}
