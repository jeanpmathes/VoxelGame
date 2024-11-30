// <copyright file="IVaryingHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
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
        void MeshVaryingHeightSide(Side side)
        {
            Vector3i checkPosition = side.Offset(position);
            BlockInstance? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;

            Boolean isFullHeight = GetHeight(info.Data) == MaximumHeight;

            if ((side != Side.Top || isFullHeight) && ISimple.IsHiddenFace(this, blockToCheck.Value, side)) return;

            MeshData mesh = GetMeshData(info with {Side = side});

            Boolean isModified = side != Side.Bottom && !isFullHeight;

            if (isModified) MeshLikeFluid(position, side, blockToCheck, info, mesh, context);
            else MeshLikeSimple(position, side, mesh, IsOpaque, IsUnshaded, context);
        }

        MeshVaryingHeightSide(Side.Front);
        MeshVaryingHeightSide(Side.Back);
        MeshVaryingHeightSide(Side.Left);
        MeshVaryingHeightSide(Side.Right);
        MeshVaryingHeightSide(Side.Bottom);
        MeshVaryingHeightSide(Side.Top);
    }

    OverlayTexture IOverlayTextureProvider.GetOverlayTexture(Content content)
    {
        MeshData mesh = GetMeshData(new BlockMeshInfo
        {
            Data = content.Block.Data,
            Fluid = content.Fluid.Fluid,
            Side = Side.Front
        });

        return new OverlayTexture
        {
            TextureIdentifier = mesh.TextureIndex,
            Tint = mesh.Tint,
            IsAnimated = false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MeshLikeSimple(
        Vector3i position, Side side, MeshData mesh, Boolean isOpaque, Boolean isUnshaded, MeshingContext context)
    {
        ISimple.AddSimpleMesh(position,
            side,
            new ISimple.MeshData
            {
                TextureIndex = mesh.TextureIndex,
                IsTextureRotated = false,
                Tint = mesh.Tint,
                IsAnimated = false
            },
            isOpaque,
            isUnshaded,
            context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MeshLikeFluid(Vector3i position, Side side, [DisallowNull] BlockInstance? blockToCheck, BlockMeshInfo info, MeshData mesh, MeshingContext context)
    {
        Int32 height = GetHeight(info.Data);

        if (side != Side.Top && blockToCheck.Value.Block is IHeightVariable toCheck &&
            toCheck.GetHeight(blockToCheck.Value.Data) == height) return;

        (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
        Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));

        if (side is not (Side.Top or Side.Bottom))
        {
            (Vector2 min, Vector2 max) bounds = GetBounds(height);
            Meshing.SetUVs(ref data, bounds.min, (bounds.min.X, bounds.max.Y), bounds.max, (bounds.max.X, bounds.min.Y));
        }
        else
        {
            Meshing.SetFullUVs(ref data);
        }

        context.GetVaryingHeightBlockMeshFaceHolder(side, IsOpaque).AddFace(
            position,
            height,
            NoHeight,
            MeshFaceHolder.DefaultDirection,
            data,
            isSingleSided: true,
            height == MaximumHeight);
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
        public Int32 TextureIndex { get; init; }

        /// <summary>
        ///     The block tint.
        /// </summary>
        public TintColor Tint { get; init; }

        /// <inheritdoc />
        public Boolean Equals(MeshData other)
        {
            return (TextureIndex, Tint) ==
                   (other.TextureIndex, other.Tint);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is MeshData other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(TextureIndex, Tint);
        }

        /// <summary>
        ///     Equality operator.
        /// </summary>
        public static Boolean operator ==(MeshData left, MeshData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Inequality operator.
        /// </summary>
        public static Boolean operator !=(MeshData left, MeshData right)
        {
            return !left.Equals(right);
        }
    }
}
