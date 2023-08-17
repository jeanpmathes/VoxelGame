// <copyright file="IFoliage.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Defines how foliage is meshed.
/// </summary>
public interface IFoliage : IBlockMeshable
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        Vector3 offset = position;

        MeshData mesh = GetMeshData(info);
        BlockMesh.Quad[] quads = mesh.Quads;
        PooledList<SpatialVertex> vertices = context.GetFoliageMesh();

        for (var index = 0; index < mesh.QuadCount; index++)
        {
            BlockMesh.Quad quad = quads[index];

            Meshing.SetTint(ref quad.data, mesh.Tint.Select(context.GetBlockTint(position)));
            Meshing.SetFlag(ref quad.data, Meshing.QuadFlag.IsAnimated, mesh.IsAnimated);

            Meshing.SetFoliageFlag(ref quad.data, Meshing.FoliageQuadFlag.IsDoublePlant, mesh.IsDoublePlant);
            Meshing.SetFoliageFlag(ref quad.data, Meshing.FoliageQuadFlag.IsUpperPart, mesh.IsUpperPart);

            Meshing.PushQuadWithOffset(vertices, quad.Positions, quad.data, offset);
        }
    }

    /// <summary>
    ///     Provides the mesh data for meshing.
    /// </summary>
    protected MeshData GetMeshData(BlockMeshInfo info);

    /// <summary>
    ///     The data that blocks have to provide for foliage meshing.
    /// </summary>
    public readonly struct MeshData : IEquatable<MeshData>
    {
        private readonly BlockMesh.Quad[] quads;
        private readonly uint quadCount;

        /// <summary>
        ///     Create the mesh data.
        /// </summary>
        public MeshData(BlockMesh mesh)
        {
            quads = mesh.GetMeshData(out quadCount);

            Tint = TintColor.None;
            IsAnimated = false;
            IsUpperPart = false;
            IsDoublePlant = false;
        }

        /// <summary>
        ///     Get the quads of the mesh.
        /// </summary>
        public BlockMesh.Quad[] Quads => quads;

        /// <summary>
        ///     Get the quad count of the mesh.
        /// </summary>
        public uint QuadCount => quadCount;

        /// <summary>
        ///     The block tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <summary>
        ///     Whether the block is animated.
        /// </summary>
        public bool IsAnimated { get; init; }

        /// <summary>
        ///     Get whether this block is the upper part.
        /// </summary>
        public bool IsUpperPart { get; init; }

        /// <summary>
        ///     Get whether this block is a double plant.
        /// </summary>
        public bool IsDoublePlant { get; init; }

        /// <summary>
        ///     Check equality.
        /// </summary>
        public bool Equals(MeshData other)
        {
            return (quads, Tint, IsAnimated, IsUpperPart, IsDoublePlant) == (other.quads, other.Tint, other.IsAnimated, other.IsUpperPart, other.IsDoublePlant);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(quads, Tint, IsAnimated, IsUpperPart, IsDoublePlant);
        }

        /// <summary>
        ///     Check equality.
        /// </summary>
        public static bool operator ==(MeshData left, MeshData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check inequality.
        /// </summary>
        public static bool operator !=(MeshData left, MeshData right)
        {
            return !left.Equals(right);
        }
    }
}
