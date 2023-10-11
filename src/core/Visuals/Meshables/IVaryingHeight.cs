﻿// <copyright file="IVaryingHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
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

            if (isModified) MeshLikeFluid(position, side, blockToCheck, info, mesh, context);
            else MeshLikeSimple(position, side, mesh, IsOpaque, context);
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
    private static void MeshLikeSimple(
        Vector3i position, BlockSide side, MeshData mesh, bool isOpaque, MeshingContext context)
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
            context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MeshLikeFluid(Vector3i position, BlockSide side, [DisallowNull] BlockInstance? blockToCheck, BlockMeshInfo info, MeshData mesh, MeshingContext context)
    {
        int height = GetHeight(info.Data);

        if (side != BlockSide.Top && blockToCheck.Value.Block is IHeightVariable toCheck &&
            toCheck.GetHeight(blockToCheck.Value.Data) == height) return;

        (uint a, uint b, uint c, uint d) data = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
        Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));

        if (side is not (BlockSide.Top or BlockSide.Bottom))
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
            MeshFaceHolder.NoSkip,
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
