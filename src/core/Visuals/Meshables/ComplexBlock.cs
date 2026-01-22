// <copyright file="ComplexBlock.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks that allow complex meshes, with the least amount of restrictions.
/// </summary>
public class ComplexBlock : Block
{
    private readonly Complex complex;

    private Complex.MeshData[] meshData = null!;

    /// <inheritdoc />
    public ComplexBlock(UInt32 blockID, CID contentID, String name) : base(blockID, contentID, name)
    {
        complex = Require<Complex>();
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Complex;

    /// <param name="validator"></param>
    /// <inheritdoc />
    protected override void OnValidate(IValidator validator) {}

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        meshData = new Complex.MeshData[States.Count];

        foreach ((State state, Int32 index) in States.AllStatesWithIndex)
        {
            if (!Constraint.IsStateValid(state))
            {
                Mesh.Quad[] quads = Meshes.CreateFallback().GetMeshData(out UInt32 count);

                meshData[index] = new Complex.MeshData(quads, count, ColorS.NoTint, IsAnimated: false);

                continue;
            }

            Complex.MeshData mesh = complex.GetMeshData(new MeshContext(state, textureIndexProvider, modelProvider, validator));
            BuildMeshData(mesh);
            meshData[index] = mesh;
        }
    }

    private void BuildMeshData(Complex.MeshData mesh)
    {
        Mesh.Quad[] quads = mesh.Quads;

        for (var index = 0; index < mesh.QuadCount; index++)
        {
            ref Mesh.Quad quad = ref quads[index];

            Meshing.SetFlag(ref quad.data, Meshing.QuadFlag.IsAnimated, mesh.IsAnimated);
            Meshing.SetFlag(ref quad.data, Meshing.QuadFlag.IsUnshaded, IsUnshaded);
        }
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        Vector3 offset = position;
        IMeshing meshing = context.GetBasicMesh(IsOpaque);

        ref readonly Complex.MeshData mesh = ref meshData[state.Index];
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
        ref readonly Complex.MeshData mesh = ref meshData[state.Index];

        if (mesh.QuadCount == 0)
            return ColorS.Black;

        Int32 textureIndex = Meshing.GetTextureIndex(ref mesh.Quads[0].data);
        ColorS color = DominantColorProvider.GetDominantColor(textureIndex, isBlock: true);

        return color * mesh.Tint.Select(positionTint);
    }
}
