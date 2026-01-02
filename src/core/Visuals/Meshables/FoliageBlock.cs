// <copyright file="FoliageBlock.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks that use specialized foliage meshing.
/// </summary>
public class FoliageBlock : Block
{
    private static readonly UInt32 errorQuadCount;
    private static readonly Mesh.Quad[] errorQuads = Meshes.CreateCrossMesh(ITextureIndexProvider.MissingTextureIndex).GetMeshData(out errorQuadCount);

    private readonly Foliage foliage;

    private Foliage.MeshData[] meshData = null!;

    /// <inheritdoc />
    public FoliageBlock(UInt32 blockID, CID contentID, String name) : base(blockID, contentID, name)
    {
        foliage = Require<Foliage>();
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Foliage;

    /// <param name="validator"></param>
    /// <inheritdoc />
    protected override void OnValidate(IValidator validator) {}

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        meshData = new Foliage.MeshData[States.Count];

        foreach ((State state, Int32 index) in States.AllStatesWithIndex)
        {
            if (!Constraint.IsStateValid(state))
            {
                meshData[index] = new Foliage.MeshData(errorQuads, errorQuadCount, ColorS.NoTint, Foliage.PartType.Single, IsAnimated: false);

                continue;
            }

            Foliage.MeshData mesh = foliage.GetMeshData(state, textureIndexProvider, visuals);
            BuildMeshData(mesh);
            meshData[index] = mesh;
        }
    }

    private void BuildMeshData(Foliage.MeshData mesh)
    {
        Mesh.Quad[] quads = mesh.Quads;

        for (var index = 0; index < mesh.QuadCount; index++)
        {
            ref Mesh.Quad quad = ref quads[index];

            Meshing.SetFlag(ref quad.data, Meshing.QuadFlag.IsAnimated, mesh.IsAnimated);
            Meshing.SetFlag(ref quad.data, Meshing.QuadFlag.IsUnshaded, IsUnshaded);

            Meshing.SetFoliageFlag(ref quad.data, Meshing.FoliageQuadFlag.IsDoublePlant, mesh.Part is Foliage.PartType.DoubleLower or Foliage.PartType.DoubleUpper);
            Meshing.SetFoliageFlag(ref quad.data, Meshing.FoliageQuadFlag.IsUpperPart, mesh.Part is Foliage.PartType.DoubleUpper);
        }
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        Vector3 offset = position;
        IMeshing meshing = context.GetFoliageMesh();

        ref readonly Foliage.MeshData mesh = ref meshData[state.Index];
        Mesh.Quad[] quads = mesh.Quads;

        for (var index = 0; index < mesh.QuadCount; index++)
        {
            ref readonly Mesh.Quad quad = ref quads[index];
            (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = quad.data;

            Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));

            meshing.PushQuadWithOffset(quad.Positions, data, offset);
        }
    }

    /// <inheritdoc />
    public override ColorS GetDominantColor(State state, ColorS positionTint)
    {
        ref readonly Foliage.MeshData mesh = ref meshData[state.Index];

        if (mesh.QuadCount == 0)
            return ColorS.Black;

        Int32 textureIndex = Meshing.GetTextureIndex(ref mesh.Quads[0].data);
        ColorS color = DominantColorProvider.GetDominantColor(textureIndex, isBlock: true);

        return color * mesh.Tint.Select(positionTint);
    }
}
