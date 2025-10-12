// <copyright file="SimpleBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks which use simple meshing which only supports full blocks.
/// </summary>
public class SimpleBlock : Logic.Voxels.Block, IOverlayTextureProvider
{
    private readonly SideArray<Simple.MeshData[]> meshData = new();
    private readonly Simple simple;

    /// <inheritdoc />
    public SimpleBlock(UInt32 blockID, CID contentID, String name) : base(blockID, contentID, name)
    {
        simple = Require<Simple>();

        BoundingVolume.ContributeConstant(Physics.BoundingVolume.Block, exclusive: true);

        Require<Overlay>().OverlayTextureProvider.ContributeConstant(this);
    }

    /// <inheritdoc />
    protected override Boolean IsAlwaysFull => true;

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Simple;

    OverlayTexture IOverlayTextureProvider.GetOverlayTexture(Content content)
    {
        Simple.MeshData mesh = meshData[Side.Front][content.Block.Index];

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
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals)
    {
        foreach (Side side in Side.All.Sides())
        {
            meshData[side] = new Simple.MeshData[States.Count];

            foreach ((State state, Int32 index) in States.GetAllStatesWithIndex())
            {
                if (!Constraint.IsStateValid(state))
                {
                    meshData[side][index] = new Simple.MeshData(ITextureIndexProvider.MissingTextureIndex, IsTextureRotated: false, ColorS.None, IsAnimated: false);

                    continue;
                }

                meshData[side][index] = simple.GetMeshData(state, side, textureIndexProvider);
            }
        }
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MeshSimpleSide(Side side)
        {
            Vector3i checkPosition = side.Offset(position);
            State? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;
            if (IsHiddenFace(this, blockToCheck.Value, side)) return;

            Simple.MeshData mesh = meshData[side][state.Index];
            AddSimpleMesh(position, side, mesh, IsOpaque, IsUnshaded, context);
        }

        MeshSimpleSide(Side.Front);
        MeshSimpleSide(Side.Back);
        MeshSimpleSide(Side.Left);
        MeshSimpleSide(Side.Right);
        MeshSimpleSide(Side.Bottom);
        MeshSimpleSide(Side.Top);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddSimpleMesh(
        Vector3i position, Side side, Simple.MeshData mesh, Boolean isOpaque, Boolean isUnshaded, MeshingContext context)
    {
        (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
        Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));
        Meshing.SetFullUVs(ref data);

        Meshing.SetFlag(ref data, Meshing.QuadFlag.IsAnimated, mesh.IsAnimated);
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
    public static Boolean IsHiddenFace(Block current, State neighbor, Side side)
    {
        Boolean blockToCheckIsConsideredOpaque = neighbor.Block.IsOpaque
                                                 || current is {IsOpaque: false, MeshFaceAtNonOpaques: false} && !neighbor.Block.MeshFaceAtNonOpaques;

        return neighbor.IsSideFull(side.Opposite()) && blockToCheckIsConsideredOpaque;
    }
}
