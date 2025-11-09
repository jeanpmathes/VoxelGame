// <copyright file="Mesh.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A mesh, capable of defining more complex shapes than just a cube.
///     The mesh is defined by a set of quads, each defined by four vertices and per-quad data.
///     In contrast to a <see cref="Model"/>, the mesh is stored in a format ready to be uploaded to the GPU.
/// </summary>
public class Mesh
{
    private readonly Quad[] quads;

    /// <summary>
    ///     Create a new mesh.
    /// </summary>
    /// <param name="quads">The quads defining the mesh.</param>
    public Mesh(Quad[] quads)
    {
        this.quads = quads;
    }

    /// <summary>
    ///     Get a copy of the mesh with the given offset applied.
    /// </summary>
    /// <param name="offset">The offset to apply.</param>
    /// <returns>The new mesh.</returns>
    public Mesh WithOffset(Vector3 offset)
    {
        Mesh mesh = new(new Quad[quads.Length]);

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
    /// <returns>A new, subdivided mesh.</returns>
    public Mesh SubdivideU()
    {
        return Subdivide(quads, DivideAlongU);
    }

    /// <summary>
    ///     Subdivide the mesh in the V direction, which is the vertical direction.
    /// </summary>
    /// <returns>A new, subdivided mesh.</returns>
    public Mesh SubdivideV()
    {
        return Subdivide(quads, DivideAlongV);
    }

    private static Mesh Subdivide(Quad[] original, Action<Int32, Mesh, Quad> divider)
    {
        Mesh mesh = new(new Quad[original.Length * 2]);

        for (var index = 0; index < original.Length; index++)
        {
            Quad quad = original[index];
            
            Int32 first = index * 2;
            mesh.quads[first] = quad;

            Int32 second = index * 2 + 1;
            mesh.quads[second] = quad;

            divider(index, mesh, quad);
        }

        return mesh;
    }

    private static void DivideAlongU(Int32 target, Mesh mesh, Quad original)
    {
        Int32 first = target * 2;
        Int32 second = target * 2 + 1;

        Vector3 midLeftPosition = (original.A + original.B) / 2;
        Vector3 midRightPosition = (original.D + original.C) / 2;

        mesh.quads[first].B = midLeftPosition;
        mesh.quads[first].C = midRightPosition;

        mesh.quads[second].A = midLeftPosition;
        mesh.quads[second].D = midRightPosition;

        (Vector2 a, Vector2 b, Vector2 c, Vector2 d) uv = Meshing.GetUVs(ref original.data);
        Vector2 midLeftUV = (uv.a + uv.b) / 2;
        Vector2 midRightUV = (uv.d + uv.c) / 2;

        Meshing.SetUVs(ref mesh.quads[first].data, uv.a, midLeftUV, midRightUV, uv.d);
        Meshing.SetUVs(ref mesh.quads[second].data, midLeftUV, uv.b, uv.c, midRightUV);
    }

    private static void DivideAlongV(Int32 target, Mesh mesh, Quad original)
    {
        Int32 first = target * 2;
        Int32 second = target * 2 + 1;

        Vector3 midBottomPosition = (original.A + original.D) / 2;
        Vector3 midTopPosition = (original.B + original.C) / 2;

        mesh.quads[first].C = midTopPosition;
        mesh.quads[first].D = midBottomPosition;

        mesh.quads[second].A = midBottomPosition;
        mesh.quads[second].B = midTopPosition;

        (Vector2 a, Vector2 b, Vector2 c, Vector2 d) uv = Meshing.GetUVs(ref original.data);
        Vector2 midBottomUV = (uv.a + uv.d) / 2;
        Vector2 midTopUV = (uv.b + uv.c) / 2;

        Meshing.SetUVs(ref mesh.quads[first].data, uv.a, uv.b, midTopUV, midBottomUV);
        Meshing.SetUVs(ref mesh.quads[second].data, midBottomUV, midTopUV, uv.c, uv.d);
    }

    /// <summary>
    ///     Combine multiple meshes into one.
    /// </summary>
    public static Mesh Combine(params IEnumerable<Mesh> meshes)
    {
        List<Quad> quads = [];

        foreach (Mesh mesh in meshes) quads.AddRange(mesh.quads);

        return new Mesh(quads.ToArray());
    }

    /// <summary>
    ///     Get the mesh data.
    /// </summary>
    /// <param name="count">The number of quads, will be set to the length of the array.</param>
    /// <returns>The mesh data.</returns>
    public Quad[] GetMeshData(out UInt32 count)
    {
        count = (UInt32) quads.Length;

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
        public (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data;

        #pragma warning restore S1104

        /// <summary>
        ///     Check whether this quad is equal to another.
        /// </summary>
        public Boolean Equals(Quad other)
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
        public override Boolean Equals(Object? obj)
        {
            return obj is Quad other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(A, B, C, D);
        }

        /// <summary>
        ///     Check equality of two quads.
        /// </summary>
        public static Boolean operator ==(Quad left, Quad right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check inequality of two quads.
        /// </summary>
        public static Boolean operator !=(Quad left, Quad right)
        {
            return !left.Equals(right);
        }
    }
}
