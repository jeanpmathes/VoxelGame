// <copyright file="BlockMesh.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A mesh for a complex block, capable of defining more complex shapes than just a cube.
///     The mesh is defined by a set of quads.
/// </summary>
public class BlockMesh
{
    private readonly Quad[] quads;

    /// <summary>
    ///     Create a new block mesh.
    /// </summary>
    /// <param name="quads">The quads defining the mesh.</param>
    public BlockMesh(Quad[] quads)
    {
        this.quads = quads;
    }

    /// <summary>
    ///     Get a copy of the mesh with the given offset applied.
    /// </summary>
    /// <param name="offset">The offset to apply.</param>
    /// <returns>The new mesh.</returns>
    public BlockMesh WithOffset(Vector3 offset)
    {
        BlockMesh mesh = new(new Quad[quads.Length]);

        for (var quad = 0; quad < quads.Length; quad++)
            mesh.quads[quad] = new Quad
            {
                A = quads[quad].A + offset,
                B = quads[quad].B + offset,
                C = quads[quad].C + offset,
                D = quads[quad].D + offset,
                data = quads[quad].data
            };

        return mesh;
    }

    /// <summary>
    ///     Subdivide the mesh in the U direction, which is the horizontal direction.
    /// </summary>
    /// <returns>The new mesh.</returns>
    public BlockMesh SubdivideU()
    {
        return Subdivide(DivideAlongU);
    }

    /// <summary>
    ///     Subdivide the mesh in the V direction, which is the vertical direction.
    /// </summary>
    /// <returns>The new mesh.</returns>
    public BlockMesh SubdivideV()
    {
        return Subdivide(DivideAlongV);
    }

    private BlockMesh Subdivide(Action<int, BlockMesh> divider)
    {
        BlockMesh mesh = new(new Quad[quads.Length * 2]);

        for (var quad = 0; quad < quads.Length; quad++)
        {
            int first = quad * 2;
            mesh.quads[first] = quads[quad];

            int second = quad * 2 + 1;
            mesh.quads[second] = quads[quad];

            divider(quad, mesh);
        }

        return mesh;
    }

    private void DivideAlongU(int quad, BlockMesh mesh)
    {
        int first = quad * 2;
        int second = quad * 2 + 1;

        Vector3 midLeftPosition = (quads[quad].A + quads[quad].B) / 2;
        Vector3 midRightPosition = (quads[quad].D + quads[quad].C) / 2;

        mesh.quads[first].B = midLeftPosition;
        mesh.quads[first].C = midRightPosition;

        mesh.quads[second].A = midLeftPosition;
        mesh.quads[second].D = midRightPosition;

        (Vector2 a, Vector2 b, Vector2 c, Vector2 d) uv = Meshing.GetUVs(ref quads[quad].data);
        Vector2 midLeftUV = (uv.a + uv.b) / 2;
        Vector2 midRightUV = (uv.d + uv.c) / 2;

        Meshing.SetUVs(ref mesh.quads[first].data, uv.a, midLeftUV, midRightUV, uv.d);
        Meshing.SetUVs(ref mesh.quads[second].data, midLeftUV, uv.b, uv.c, midRightUV);
    }

    private void DivideAlongV(int quad, BlockMesh mesh)
    {
        int first = quad * 2;
        int second = quad * 2 + 1;

        Vector3 midBottomPosition = (quads[quad].A + quads[quad].D) / 2;
        Vector3 midTopPosition = (quads[quad].B + quads[quad].C) / 2;

        mesh.quads[first].C = midTopPosition;
        mesh.quads[first].D = midBottomPosition;

        mesh.quads[second].A = midBottomPosition;
        mesh.quads[second].B = midTopPosition;

        (Vector2 a, Vector2 b, Vector2 c, Vector2 d) uv = Meshing.GetUVs(ref quads[quad].data);
        Vector2 midBottomUV = (uv.a + uv.d) / 2;
        Vector2 midTopUV = (uv.b + uv.c) / 2;

        Meshing.SetUVs(ref mesh.quads[first].data, uv.a, uv.b, midTopUV, midBottomUV);
        Meshing.SetUVs(ref mesh.quads[second].data, midBottomUV, midTopUV, uv.c, uv.d);
    }

    /// <summary>
    ///     Get the mesh as mesh data for complex meshing.
    /// </summary>
    /// <param name="tint">An optional tint.</param>
    /// <param name="isAnimated">Whether the model is animated.</param>
    /// <returns>The mesh data.</returns>
    public IComplex.MeshData GetMeshData(TintColor? tint = null, bool isAnimated = false)
    {
        return new IComplex.MeshData(quads)
        {
            Tint = tint ?? TintColor.None,
            IsAnimated = isAnimated
        };
    }

    /// <summary>
    ///     Get the mesh data.
    /// </summary>
    /// <param name="count">The number of quads, will be set to the length of the array.</param>
    /// <returns>The mesh data.</returns>
    public Quad[] GetMeshData(out uint count)
    {
        count = (uint) quads.Length;

        return quads;
    }

    /// <summary>
    ///     A quad is defined by four vertices and their data.
    ///     Vertices are defined in clockwise order.
    /// </summary>
    public struct Quad : IEquatable<Quad>
    {
        /// <summary>
        ///     The first vertex.
        /// </summary>
        public Vector3 A { get; set; }

        /// <summary>
        ///     The second vertex.
        /// </summary>
        public Vector3 B { get; set; }

        /// <summary>
        ///     The third vertex.
        /// </summary>
        public Vector3 C { get; set; }

        /// <summary>
        ///     The fourth vertex.
        /// </summary>
        public Vector3 D { get; set; }

        /// <summary>
        ///     Get all positions of the quad.
        /// </summary>
        public (Vector3, Vector3, Vector3, Vector3) Positions => (A, B, C, D);

        #pragma warning disable S1104 // Required to pass it by reference.

        /// <summary>
        ///     The data of the quad.
        /// </summary>
        public (uint a, uint b, uint c, uint d) data;

        #pragma warning restore S1104

        /// <summary>
        ///     Check whether this quad is equal to another.
        /// </summary>
        public bool Equals(Quad other)
        {
            #pragma warning disable S1067
            return A.Equals(other.A)
                   && B.Equals(other.B)
                   && C.Equals(other.C)
                   && D.Equals(other.D)
                   && data.Equals(other.data);
            #pragma warning restore S1067
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Quad other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C, D);
        }

        /// <summary>
        ///     Check equality of two quads.
        /// </summary>
        public static bool operator ==(Quad left, Quad right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check inequality of two quads.
        /// </summary>
        public static bool operator !=(Quad left, Quad right)
        {
            return !left.Equals(right);
        }
    }
}
