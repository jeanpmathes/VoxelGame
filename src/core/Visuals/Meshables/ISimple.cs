// <copyright file="ISimple.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     The meshable for blocks that use only simple full faces.
/// </summary>
public interface ISimple : IBlockMeshable, IOverlayTextureProvider
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MeshSimpleSide(BlockSide side)
        {
            Vector3i checkPosition = side.Offset(position);
            BlockInstance? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;
            if (IsHiddenFace(this, blockToCheck.Value, side)) return;

            MeshData mesh = GetMeshData(info with {Side = side});

            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);
            int[][] uvs = BlockModels.GetBlockUVs(mesh.IsTextureRotated);

            (int x, int y, int z) = position;

            // int: uv-- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position)
            int upperDataA = (uvs[0][0] << 31) | (uvs[0][1] << 30) | ((a[0] + x) << 10) |
                             ((a[1] + y) << 5) | (a[2] + z);

            int upperDataB = (uvs[1][0] << 31) | (uvs[1][1] << 30) | ((b[0] + x) << 10) |
                             ((b[1] + y) << 5) | (b[2] + z);

            int upperDataC = (uvs[2][0] << 31) | (uvs[2][1] << 30) | ((c[0] + x) << 10) |
                             ((c[1] + y) << 5) | (c[2] + z);

            int upperDataD = (uvs[3][0] << 31) | (uvs[3][1] << 30) | ((d[0] + x) << 10) |
                             ((d[1] + y) << 5) | (d[2] + z);

            // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
            int lowerData = (mesh.Tint.GetBits(context.GetBlockTint(position)) << 23) | ((int) side << 18) |
                            mesh.GetAnimationBit(shift: 16) | mesh.TextureIndex;

            context.GetSimpleBlockMeshFaceHolder(side).AddFace(
                position,
                lowerData,
                (upperDataA, upperDataB, upperDataC, upperDataD),
                mesh.IsTextureRotated);
        }

        MeshSimpleSide(BlockSide.Front);
        MeshSimpleSide(BlockSide.Back);
        MeshSimpleSide(BlockSide.Left);
        MeshSimpleSide(BlockSide.Right);
        MeshSimpleSide(BlockSide.Bottom);
        MeshSimpleSide(BlockSide.Top);
    }


    /// <inheritdoc />
    void IBlockMeshable.Validate()
    {
        Debug.Assert(IsFull, "Simple blocks must be full.");
    }

    OverlayTexture IOverlayTextureProvider.GetOverlayTexture(Content content)
    {
        MeshData mesh = GetMeshData(new BlockMeshInfo
        {
            Data = content.Block.Data,
            Fluid = content.Fluid.Fluid,
            Side = BlockSide.Front
        });

        return new OverlayTexture
        {
            TextureIdentifier = mesh.TextureIndex,
            Tint = mesh.Tint,
            IsAnimated = mesh.IsAnimated
        };
    }

    /// <summary>
    ///     Check whether the current face is hidden according to the meshing rules for simple blocks.
    /// </summary>
    /// <param name="current">The current block.</param>
    /// <param name="neighbor">The neighboring block instance.</param>
    /// <param name="side">The side of the current block that is being checked.</param>
    /// <returns>True if the face is hidden, false otherwise.</returns>
    public static bool IsHiddenFace(IBlockBase current, BlockInstance neighbor, BlockSide side)
    {
        bool blockToCheckIsConsideredOpaque = neighbor.Block.IsOpaque
                                              || (current is {IsOpaque: false, RenderFaceAtNonOpaques: false} && !neighbor.Block.RenderFaceAtNonOpaques);

        return neighbor.IsSideFull(side.Opposite()) && blockToCheckIsConsideredOpaque;
    }

    /// <summary>
    ///     Provides the mesh data.
    /// </summary>
    protected MeshData GetMeshData(BlockMeshInfo info);

    /// <summary>
    ///     Create mesh data for a basic block.
    /// </summary>
    protected static MeshData CreateData(int textureIndex, bool isTextureRotated)
    {
        return new MeshData
        {
            TextureIndex = textureIndex,
            IsTextureRotated = isTextureRotated,
            Tint = TintColor.None,
            IsAnimated = false
        };
    }

    /// <summary>
    ///     The mesh data required for meshing with the <see cref="ISimple" /> interface.
    /// </summary>
    protected readonly struct MeshData : IEquatable<MeshData>
    {
        /// <summary>
        ///     Get the texture index.
        /// </summary>
        public int TextureIndex { get; init; }

        /// <summary>
        ///     Whether the texture is rotated.
        /// </summary>
        public bool IsTextureRotated { get; init; }

        /// <summary>
        ///     The block tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <summary>
        ///     Whether the block is animated.
        /// </summary>
        public bool IsAnimated { get; init; }

        /// <summary>
        ///     Get the animation bit.
        /// </summary>
        public int GetAnimationBit(int shift)
        {
            return IsAnimated && TextureIndex != 0 ? 1 << shift : 0;
        }

        /// <inheritdoc />
        public bool Equals(MeshData other)
        {
            return (TextureIndex, IsTextureRotated, Tint, IsAnimated) ==
                   (other.TextureIndex, other.IsTextureRotated, other.Tint, other.IsAnimated);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(TextureIndex, IsTextureRotated, Tint, IsAnimated);
        }

        /// <summary>
        ///     Equality operator.
        /// </summary>
        public static bool operator ==(MeshData left, MeshData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Inequality operator.
        /// </summary>
        public static bool operator !=(MeshData left, MeshData right)
        {
            return !left.Equals(right);
        }
    }
}


