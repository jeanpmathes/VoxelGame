// <copyright file="SimpleBlock.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
///     Blocks which use simple meshing which only supports full blocks.
/// </summary>
public class SimpleBlock : Block, IOverlayTextureProvider
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

    /// <param name="validator"></param>
    /// <inheritdoc />
    protected override void OnValidate(IValidator validator) {}

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        foreach (Side side in Side.All.Sides())
        {
            meshData[side] = new Simple.MeshData[States.Count];

            foreach ((State state, Int32 index) in States.AllStatesWithIndex)
            {
                if (!Constraint.IsStateValid(state))
                {
                    meshData[side][index] = new Simple.MeshData(ITextureIndexProvider.MissingTextureIndex, IsTextureRotated: false, ColorS.NoTint, IsAnimated: false);

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
            Vector3i checkPosition = position.Offset(side);
            State? blockToCheck = context.GetBlock(checkPosition, side);

            if (blockToCheck == null) return;
            if (IsHiddenFace(this, blockToCheck.Value, side)) return;

            ref readonly Simple.MeshData mesh = ref meshData[side][state.Index];
            AddSimpleMesh(position, side, in mesh, IsOpaque, IsUnshaded, context);
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
        Vector3i position, Side side, ref readonly Simple.MeshData mesh, Boolean isOpaque, Boolean isUnshaded, MeshingContext context)
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

    /// <inheritdoc />
    public override ColorS GetDominantColor(State state, ColorS positionTint)
    {
        ref readonly Simple.MeshData mesh = ref meshData[Side.Front][state.Index];

        ColorS color = DominantColorProvider.GetDominantColor(mesh.TextureIndex, isBlock: true);

        return color * mesh.Tint.Select(positionTint);
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
                                                 || (current is {IsOpaque: false, MeshFaceAtNonOpaques: false} && !neighbor.Block.MeshFaceAtNonOpaques);

        return neighbor.IsSideFull(side.Opposite()) && blockToCheckIsConsideredOpaque;
    }
}
