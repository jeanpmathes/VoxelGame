// <copyright file="ComplexBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks that allow complex meshes, with the least amount of restrictions.
/// </summary>
public class ComplexBlock : Block
{
    private readonly Complex complex;

    private Complex.MeshData[] meshData = null!;

    /// <inheritdoc />
    public ComplexBlock(UInt32 id, String namedID, String name) : base(id, namedID, name)
    {
        complex = Require<Complex>();
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Complex;

    /// <inheritdoc />
    protected override void OnValidate() {}

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals)
    {
        meshData = new Complex.MeshData[States.Count];

        foreach ((State state, Int32 index) in States.GetAllStatesWithIndex())
        {
            if (!Constraint.IsStateValid(state))
            {
                Mesh.Quad[] quads = Meshes.CreateFallback().GetMeshData(out UInt32 count);

                meshData[index] = new Complex.MeshData(quads, count, ColorS.None, IsAnimated: false);

                continue;
            }

            Complex.MeshData mesh = complex.GetMeshData(state, textureIndexProvider, modelProvider, visuals);
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
}
