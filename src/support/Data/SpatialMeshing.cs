// <copyright file="SpatialMeshing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Support.Data;

/// <summary>
///     Builds a mesh for <see cref="VoxelGame.Support.Objects.Mesh" />.
/// </summary>
public class SpatialMeshing : IMeshing
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
        int size = primitive switch
        {
            IMeshing.Primitive.Quad => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, message: null)
        };

        mesh.EnsureCapacity(mesh.Count + size * count);
    }

    /// <inheritdoc />
    public int Count => mesh.Count;

    /// <inheritdoc />
    public void Release()
    {
        mesh.ReturnToPool();
    }
}
