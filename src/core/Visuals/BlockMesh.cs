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
            return HashCode.Combine(A, B, C, D, data);
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
