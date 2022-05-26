// <copyright file="ICropPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Defines how crop plants are meshed.
/// </summary>
public interface ICropPlant : IBlockMeshable
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        (int x, int y, int z) = position;
        MeshData mesh = GetMeshData(info);

        // int: o--- ssss ---- ---- -xxx xxyy yyyz zzzz (o: orientation; s: shift, xyz: position)
        int upperData = (x << 10) | (y << 5) | z;

        // int: tttt tttt tulh ---c ---i iiii iiii iiii (t: tint; u: has upper; l: lowered; h: height; c: crop type; i: texture index)
        int lowerData = (mesh.Tint.GetBits(context.BlockTint) << 23) | ((mesh.HasUpper ? 1 : 0) << 22) |
                        ((mesh.IsLowered ? 1 : 0) << 21) | ((mesh.IsUpper ? 1 : 0) << 20) |
                        ((mesh.IsDoubleCropPlant ? 1 : 0) << 16) | mesh.TextureIndex;

        PooledList<int> cropPlantVertexData = context.GetCropPlantVertexData();

        if (!mesh.IsDoubleCropPlant)
        {
            cropPlantVertexData.Add((4 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            cropPlantVertexData.Add((8 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            cropPlantVertexData.Add((12 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            const int o = 1 << 31;

            cropPlantVertexData.Add(o | (4 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            cropPlantVertexData.Add(o | (8 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            cropPlantVertexData.Add(o | (12 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);
        }
        else
        {
            cropPlantVertexData.Add((4 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            cropPlantVertexData.Add((12 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            const int o = 1 << 31;

            cropPlantVertexData.Add(o | (4 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);

            cropPlantVertexData.Add(o | (12 << 24) | upperData);
            cropPlantVertexData.Add(lowerData);
        }
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
        ///     Get the tint.
        /// </summary>
        public TintColor Tint { get; init; } = TintColor.None;

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
        ///     Get whether this block is a double crop plant.
        /// </summary>
        public bool IsDoubleCropPlant { get; init; }

        /// <summary>
        ///     Get the texture index.
        /// </summary>
        public int TextureIndex { get; init; }

        /// <inheritdoc />
        public bool Equals(MeshData other)
        {
            return (Tint, HasUpper, IsLowered, IsUpper, IsDoubleCropPlant, TextureIndex) == (other.Tint, other.HasUpper,
                other.IsLowered,
                other.IsUpper, other.IsDoubleCropPlant, other.TextureIndex);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Tint, HasUpper, IsLowered, IsUpper, IsDoubleCropPlant, TextureIndex);
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
