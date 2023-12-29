// <copyright file="IComplex.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     A meshable for complex meshes that have few constraints.
/// </summary>
public interface IComplex : IBlockMeshable
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        Vector3 offset = position;

        MeshData mesh = GetMeshData(info);
        BlockMesh.Quad[] quads = mesh.Quads;
        IMeshing meshing = context.GetBasicMesh(IsOpaque);

        for (var index = 0; index < mesh.QuadCount; index++)
        {
            BlockMesh.Quad quad = quads[index];

            Meshing.SetTint(ref quad.data, mesh.Tint.Select(context.GetBlockTint(position)));
            Meshing.SetFlag(ref quad.data, Meshing.QuadFlag.IsAnimated, mesh.IsAnimated);

            meshing.PushQuadWithOffset(quad.Positions, quad.data, offset);
        }
    }

    /// <summary>
    ///     Provides the mesh data for meshing.
    /// </summary>
    protected MeshData GetMeshData(BlockMeshInfo info);

    /// <summary>
    ///     The data that blocks have to provide for complex meshing.
    /// </summary>
    public readonly struct MeshData : IEquatable<MeshData>
    {
        private readonly BlockMesh.Quad[] quads;

        /// <summary>
        ///     Create the mesh data.
        /// </summary>
        public MeshData(BlockMesh.Quad[] quads)
        {
            this.quads = quads;

            QuadCount = (uint) quads.Length;

            Tint = TintColor.None;
            IsAnimated = false;
        }

        /// <summary>
        ///     Get the quads of the mesh.
        /// </summary>
        public BlockMesh.Quad[] Quads => quads;

        /// <summary>
        ///     Get the quad count of the mesh.
        /// </summary>
        public uint QuadCount { get; }

        /// <summary>
        ///     The block tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <summary>
        ///     Whether the block is animated.
        /// </summary>
        public bool IsAnimated { get; init; }

        /// <inheritdoc />
        public bool Equals(MeshData other)
        {
            return (Tint, IsAnimated, quads) ==
                   (other.Tint, other.IsAnimated, quads);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(quads, Tint, IsAnimated);
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static bool operator ==(MeshData left, MeshData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static bool operator !=(MeshData left, MeshData right)
        {
            return !left.Equals(right);
        }
    }
}
