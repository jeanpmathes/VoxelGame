// <copyright file="IVaryingHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Defines meshing for blocks that have varying height.
/// </summary>
public interface IVaryingHeight : IBlockMeshable, IHeightVariable, IOverlayTextureProvider
{
    void IBlockMeshable.CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MeshVaryingHeightSide(BlockSide side)
        {
            Vector3i checkPosition = side.Offset(position);
            BlockInstance? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;

            bool isFullHeight = GetHeight(info.Data) == MaximumHeight;

            if ((side != BlockSide.Top || isFullHeight) && ISimple.IsHiddenFace(this, blockToCheck.Value, side)) return;

            MeshData mesh = GetMeshData(info with {Side = side});

            bool isModified = side != BlockSide.Bottom && !isFullHeight;

            if (isModified)
            {
                MeshLikeFluid(position, side, blockToCheck, info, mesh, context);
            }
            else
            {
                MeshLikeSimple(position, side, mesh, context);
            }
        }

        MeshVaryingHeightSide(BlockSide.Front);
        MeshVaryingHeightSide(BlockSide.Back);
        MeshVaryingHeightSide(BlockSide.Left);
        MeshVaryingHeightSide(BlockSide.Right);
        MeshVaryingHeightSide(BlockSide.Bottom);
        MeshVaryingHeightSide(BlockSide.Top);
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
            IsAnimated = false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MeshLikeSimple(Vector3i position, BlockSide side, MeshData mesh, MeshingContext context)
    {
        side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);
        (int x, int y, int z) = position;

        // int: uv-- ---- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position)
        int upperDataA = (0 << 31) | (0 << 30) | ((a[0] + x) << 10) |
                         ((a[1] + y) << 5) | (a[2] + z);

        int upperDataB = (0 << 31) | (1 << 30) | ((b[0] + x) << 10) |
                         ((b[1] + y) << 5) | (b[2] + z);

        int upperDataC = (1 << 31) | (1 << 30) | ((c[0] + x) << 10) |
                         ((c[1] + y) << 5) | (c[2] + z);

        int upperDataD = (1 << 31) | (0 << 30) | ((d[0] + x) << 10) |
                         ((d[1] + y) << 5) | (d[2] + z);

        // int: tttt tttt t--n nn-_ ---i iiii iiii iiii (t: tint; n: normal; i: texture index, _: used for simple blocks but not here)
        int lowerData = (mesh.Tint.GetBits(context.GetBlockTint(position)) << 23) | ((int) side << 18) |
                        mesh.TextureIndex;

        context.GetSimpleBlockMeshFaceHolder(side).AddFace(
            position,
            lowerData,
            (upperDataA, upperDataB, upperDataC, upperDataD),
            isRotated: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MeshLikeFluid(Vector3i position, BlockSide side, [DisallowNull] BlockInstance? blockToCheck, BlockMeshInfo info, MeshData mesh, MeshingContext context)
    {
        side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);
        (int x, int y, int z) = position;

        int height = GetHeight(info.Data);

        if (side != BlockSide.Top && blockToCheck.Value.Block is IHeightVariable toCheck &&
            toCheck.GetHeight(blockToCheck.Value.Data) == height) return;

        // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; hl: texture repetition; xyz: position; e: lower/upper end)
        int upperDataA = (0 << 31) | (0 << 30) | ((x + a[0]) << 10) | (a[1] << 9) |
                         (y << 5) | (z + a[2]);

        int upperDataB = (0 << 31) | (1 << 30) | ((x + b[0]) << 10) | (b[1] << 9) |
                         (y << 5) | (z + b[2]);

        int upperDataC = (1 << 31) | (1 << 30) | ((x + c[0]) << 10) | (c[1] << 9) |
                         (y << 5) | (z + c[2]);

        int upperDataD = (1 << 31) | (0 << 30) | ((x + d[0]) << 10) | (d[1] << 9) |
                         (y << 5) | (z + d[2]);

        // int: tttt tttt tnnn hhhh ---i iiii iiii iiii (t: tint; n: normal; h: height; i: texture index)
        int lowerData = (mesh.Tint.GetBits(context.GetBlockTint(position)) << 23) | ((int) side << 20) |
                        (height << 16) | mesh.TextureIndex;

        context.GetVaryingHeightMeshFaceHolder(side).AddFace(
            position,
            lowerData,
            (upperDataA, upperDataB, upperDataC, upperDataD),
            isSingleSided: true,
            isFull: false);
    }

    /// <summary>
    ///     Provides the necessary block mesh data for the block.
    /// </summary>
    protected MeshData GetMeshData(BlockMeshInfo info);

    /// <summary>
    ///     The mesh data required for meshing with the <see cref="IVaryingHeight" /> interface.
    /// </summary>
    protected readonly struct MeshData : IEquatable<MeshData>
    {
        /// <summary>
        ///     Get the texture index.
        /// </summary>
        public int TextureIndex { get; init; }

        /// <summary>
        ///     The block tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <inheritdoc />
        public bool Equals(MeshData other)
        {
            return (TextureIndex, Tint) ==
                   (other.TextureIndex, other.Tint);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(TextureIndex, Tint);
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


