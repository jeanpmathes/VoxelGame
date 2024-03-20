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

            AddSimpleMesh(position, side, mesh, IsOpaque, IsUnshaded, context);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddSimpleMesh(
        Vector3i position, BlockSide side, MeshData mesh, bool isOpaque, bool isUnshaded, MeshingContext context)
    {
        (uint a, uint b, uint c, uint d) data = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
        Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));
        Meshing.SetFullUVs(ref data);

        Meshing.SetFlag(ref data, Meshing.QuadFlag.IsAnimated, mesh.IsActuallyAnimated);
        Meshing.SetFlag(ref data, Meshing.QuadFlag.IsTextureRotated, mesh.IsTextureRotated);
        Meshing.SetFlag(ref data, Meshing.QuadFlag.IsUnshaded, isUnshaded);

        context.GetFullBlockMeshFaceHolder(side, isOpaque).AddFace(
            position,
            data,
            mesh.IsTextureRotated,
            isSingleSided: true);
    }

    /// <summary>
    ///     Check whether the current face is hidden according to the meshing rules for simple blocks.
    /// </summary>
    /// <param name="current">The current block.</param>
    /// <param name="neighbor">The neighboring block instance.</param>
    /// <param name="side">The side of the current block that is being checked.</param>
    /// <returns>True if the face is hidden, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    protected internal readonly struct MeshData : IEquatable<MeshData>
    {
        /// <summary>
        ///     Get the texture index.
        /// </summary>
        internal int TextureIndex { get; init; }

        /// <summary>
        ///     Whether the texture is rotated.
        /// </summary>
        internal bool IsTextureRotated { get; init; }

        /// <summary>
        ///     The block tint.
        /// </summary>
        internal TintColor Tint { get; init; }

        /// <summary>
        ///     Whether the block is animated.
        /// </summary>
        internal bool IsAnimated { get; init; }

        /// <summary>
        ///     Whether the block is actually animated, meaning animation is safe.
        /// </summary>
        internal bool IsActuallyAnimated => IsAnimated && TextureIndex != 0;

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
