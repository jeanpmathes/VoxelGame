// <copyright file="ICrossPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Defines meshing for cross-plants.
/// </summary>
public interface ICrossPlant : IBlockMeshable
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        (int x, int y, int z) = position;
        MeshData mesh = GetMeshData(info);

        // todo: link to wiki instead of this comment, and maybe refactor to common utility, add inline attribute
        // int: ---- ---- ---- ---- -xxx xxyy yyyz zzzz (xyz: position)
        int upperData = (x << 10) | (y << 5) | z;

        // todo: link to wiki instead of this comment, and maybe refactor to common utility, add inline attribute
        // int: tttt tttt tulh ---- ---i iiii iiii iiii (t: tint; u: has upper; l: lowered; h: height; i: texture index)
        int lowerData = (mesh.Tint.GetBits(context.GetBlockTint(position)) << 23) | ((mesh.HasUpper ? 1 : 0) << 22) |
                        ((mesh.IsLowered ? 1 : 0) << 21) | ((mesh.IsUpper ? 1 : 0) << 20) |
                        mesh.TextureIndex;

        PooledList<int> vertexData = context.GetCrossPlantVertexData();

        vertexData.Add(upperData);
        vertexData.Add(lowerData);
    }

    /// <summary>
    ///     Provides the mesh data for meshing.
    /// </summary>
    protected MeshData GetMeshData(BlockMeshInfo info);

    /// <summary>
    ///     Contains all necessary data defining a mesh for a cross-plant.
    /// </summary>
    protected readonly struct MeshData : IEquatable<MeshData>
    {
        /// <summary>
        ///     Creates a new mesh data instance.
        /// </summary>
        public MeshData(int textureIndex)
        {
            TextureIndex = textureIndex;

            HasUpper = false;
            IsLowered = false;
            IsUpper = false;

            Tint = TintColor.None;
        }

        /// <summary>
        ///     Get the tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <summary>
        ///     Get whether this plant has an upper part.
        /// </summary>
        public bool HasUpper { get; init; }

        /// <summary>
        ///     Get whether this block is lowered a bit.
        /// </summary>
        public bool IsLowered { get; init; }

        /// <summary>
        ///     Get whether this block is the upper part.
        /// </summary>
        public bool IsUpper { get; init; }

        /// <summary>
        ///     Get the texture index.
        /// </summary>
        public int TextureIndex { get; }

        /// <inheritdoc />
        public bool Equals(MeshData other)
        {
            return (Tint, HasUpper, IsLowered, IsUpper, TextureIndex) == (other.Tint, other.HasUpper, other.IsLowered,
                other.IsUpper, other.TextureIndex);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Tint, HasUpper, IsLowered, IsUpper, TextureIndex);
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
