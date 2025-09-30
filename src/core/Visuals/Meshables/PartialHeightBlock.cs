// <copyright file="PartialHeightBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks that are generally full blocks but can have partial heights too, even varying depending on block state.
/// </summary>
public class PartialHeightBlock : Block, IOverlayTextureProvider
{
    private readonly SideArray<PartialHeight.MeshData[]> meshData = new();
    private readonly Logic.Elements.Behaviors.Height.PartialHeight partialHeightBehavior;
    private readonly PartialHeight partialHeightMeshable;

    /// <inheritdoc />
    public PartialHeightBlock(UInt32 id, String namedID, String name) : base(id, namedID, name)
    {
        partialHeightMeshable = Require<PartialHeight>();
        partialHeightBehavior = Require<Logic.Elements.Behaviors.Height.PartialHeight>();

        Require<Overlay>().OverlayTextureProvider.ContributeConstant(this);
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.PartialHeight;

    OverlayTexture IOverlayTextureProvider.GetOverlayTexture(Content content)
    {
        PartialHeight.MeshData mesh = meshData[Side.Front][content.Block.Index];

        return new OverlayTexture
        {
            TextureIndex = mesh.TextureIndex,
            Tint = mesh.Tint,
            IsAnimated = mesh.IsAnimated
        };
    }

    /// <inheritdoc />
    protected override void OnValidate() {}

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals)
    {
        foreach (Side side in Side.All.Sides())
        {
            meshData[side] = new PartialHeight.MeshData[States.Count];

            foreach ((State state, Int32 index) in States.GetAllStatesWithIndex())
            {
                if (!Constraint.IsStateValid(state))
                {
                    meshData[side][index] = new PartialHeight.MeshData(ITextureIndexProvider.MissingTextureIndex, ColorS.None, IsAnimated: false);

                    continue;
                }

                meshData[side][index] = partialHeightMeshable.GetMeshData(state, side, textureIndexProvider);
            }
        }
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MeshVaryingHeightSide(Side side)
        {
            Vector3i checkPosition = side.Offset(position);
            State? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;

            Int32 height = partialHeightBehavior.GetHeight(state); // todo: maybe use a struct for height instead of Int32
            Boolean isFullHeight = height == Logic.Elements.Behaviors.Height.PartialHeight.MaximumHeight;

            if ((side != Side.Top || isFullHeight) && SimpleBlock.IsHiddenFace(this, blockToCheck.Value, side)) return;

            PartialHeight.MeshData mesh = meshData[side][state.Index];

            Boolean isModified = side != Side.Bottom && !isFullHeight;

            if (isModified) MeshLikeFluid(position, side, blockToCheck, height, mesh, context);
            else MeshLikeSimple(position, side, mesh, IsOpaque, IsUnshaded, context);
        }

        MeshVaryingHeightSide(Side.Front);
        MeshVaryingHeightSide(Side.Back);
        MeshVaryingHeightSide(Side.Left);
        MeshVaryingHeightSide(Side.Right);
        MeshVaryingHeightSide(Side.Bottom);
        MeshVaryingHeightSide(Side.Top);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MeshLikeSimple(
        Vector3i position, Side side, PartialHeight.MeshData mesh, Boolean isOpaque, Boolean isUnshaded, MeshingContext context)
    {
        SimpleBlock.AddSimpleMesh(position,
            side,
            new Simple.MeshData
            {
                TextureIndex = mesh.TextureIndex,
                IsTextureRotated = mesh.IsTextureRotated,
                Tint = mesh.Tint,
                IsAnimated = mesh.IsAnimated
            },
            isOpaque,
            isUnshaded,
            context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MeshLikeFluid(Vector3i position, Side side, [DisallowNull] State? blockToCheck, Int32 height, PartialHeight.MeshData mesh, MeshingContext context)
    {
        if (side != Side.Top && blockToCheck.Value.Block.Get<Logic.Elements.Behaviors.Height.PartialHeight>() is {} toCheck &&
            toCheck.GetHeight(blockToCheck.Value) == height) return;

        (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
        Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));

        // todo: allow animation here, set the bit, add it to the wiki, check that shader supports it

        if (side is not (Side.Top or Side.Bottom))
        {
            (Vector2 min, Vector2 max) bounds = PartialHeight.GetBounds(height);
            Meshing.SetUVs(ref data, bounds.min, (bounds.min.X, bounds.max.Y), bounds.max, (bounds.max.X, bounds.min.Y));
        }
        else
        {
            Meshing.SetFullUVs(ref data);
        }

        context.GetVaryingHeightBlockMeshFaceHolder(side, IsOpaque).AddFace(
            position,
            height,
            Logic.Elements.Behaviors.Height.PartialHeight.NoHeight,
            MeshFaceHolder.DefaultDirection,
            data,
            isSingleSided: true,
            height == Logic.Elements.Behaviors.Height.PartialHeight.MaximumHeight);
    }
}
