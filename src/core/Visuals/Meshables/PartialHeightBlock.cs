// <copyright file="PartialHeightBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks that are generally full blocks but can have partial heights too, even varying depending on block state.
/// </summary>
public class PartialHeightBlock : Block, IOverlayTextureProvider
{
    private readonly SideArray<PartialHeight.MeshData[]> meshData = new();
    private readonly Logic.Voxels.Behaviors.Height.PartialHeight partialHeightBehavior;
    private readonly PartialHeight partialHeightMeshable;

    /// <inheritdoc />
    public PartialHeightBlock(UInt32 blockID, CID contentID, String name) : base(blockID, contentID, name)
    {
        partialHeightMeshable = Require<PartialHeight>();
        partialHeightBehavior = Require<Logic.Voxels.Behaviors.Height.PartialHeight>();

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

    /// <param name="validator"></param>
    /// <inheritdoc />
    protected override void OnValidate(IValidator validator) {}

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        foreach (Side side in Side.All.Sides())
        {
            meshData[side] = new PartialHeight.MeshData[States.Count];

            foreach ((State state, Int32 index) in States.AllStatesWithIndex)
            {
                if (!Constraint.IsStateValid(state))
                {
                    meshData[side][index] = new PartialHeight.MeshData(ITextureIndexProvider.MissingTextureIndex, ColorS.NoTint, IsAnimated: false);

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
            Vector3i checkPosition = position.Offset(side);
            State? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;

            BlockHeight height = partialHeightBehavior.GetHeight(state);
            Boolean isFullHeight = height.IsFull;

            if ((side != Side.Top || isFullHeight) && SimpleBlock.IsHiddenFace(this, blockToCheck.Value, side)) return;

            ref readonly PartialHeight.MeshData mesh = ref meshData[side][state.Index];

            Boolean isModified = side != Side.Bottom && !isFullHeight;

            if (isModified) MeshLikeFluid(position, side, blockToCheck, height, in mesh, context);
            else MeshLikeSimple(position, side, in mesh, IsOpaque, IsUnshaded, context);
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
        Vector3i position, Side side, ref readonly PartialHeight.MeshData mesh, Boolean isOpaque, Boolean isUnshaded, MeshingContext context)
    {
        var convertedMesh = new Simple.MeshData
        {
            TextureIndex = mesh.TextureIndex,
            IsTextureRotated = PartialHeight.MeshData.IsTextureRotated,
            Tint = mesh.Tint,
            IsAnimated = mesh.IsAnimated
        };
        
        SimpleBlock.AddSimpleMesh(position,
            side,
            in convertedMesh,
            isOpaque,
            isUnshaded,
            context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MeshLikeFluid(Vector3i position, Side side, [DisallowNull] State? blockToCheck, BlockHeight height, ref readonly PartialHeight.MeshData mesh, MeshingContext context)
    {
        if (side != Side.Top && blockToCheck.Value.Block.Get<Logic.Voxels.Behaviors.Height.PartialHeight>() is {} toCheck &&
            toCheck.GetHeight(blockToCheck.Value) == height) return;

        (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
        
        Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));
        Meshing.SetFlag(ref data, Meshing.QuadFlag.IsAnimated, mesh.IsAnimated);

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
            height.ToInt32(),
            BlockHeight.None.ToInt32(),
            MeshFaceHolder.DefaultDirection,
            data,
            isSingleSided: true,
            height.IsFull);
    }

    /// <inheritdoc />
    public override ColorS GetDominantColor(State state, ColorS positionTint)
    {
        ref readonly PartialHeight.MeshData mesh = ref meshData[Side.Front][state.Index];

        ColorS color = DominantColorProvider.GetDominantColor(mesh.TextureIndex, isBlock: true);
        
        return color * mesh.Tint.Select(positionTint);
    }
}
