// <copyright file="ComplexBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks that allow complex meshes, with the least amount of restrictions.
/// </summary>
public class ComplexBlock : Block
{
    private readonly Complex complex;

    private Complex.MeshData[] meshData = null!;
    
    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Complex;

    /// <inheritdoc />
    public ComplexBlock(String name, UInt32 id, String namedID) : base(name, id, namedID)
    {
        complex = Require<Complex>();
    }

    /// <inheritdoc />
    protected override void OnValidate()
    {
        
    }

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals)
    {
        meshData = new Complex.MeshData[States.Count];
        
        foreach ((State state, Int32 index) in States.GetAllStatesWithIndex())
        {
            if (!Constraint.IsStateValid(state))
            {
                BlockMesh.Quad[] quads = BlockModels.CreateFallback().CreateMesh(textureIndexProvider).GetMeshData(out UInt32 count); // todo: create a method to get fallback model easier and without texture provider, do it in static constructor instead of in loop
                
                meshData[index] = new Complex.MeshData(quads, count, ColorS.None, IsAnimated: false);
                continue;
            }

            Complex.MeshData mesh = complex.GetMeshData(state, textureIndexProvider, blockModelProvider, visuals);
            BuildMeshData(mesh);
            meshData[index] = mesh;
        }
    }
    
    private void BuildMeshData(Complex.MeshData mesh)
    {
        BlockMesh.Quad[] quads = mesh.Quads;

        for (var index = 0; index < mesh.QuadCount; index++)
        {
            BlockMesh.Quad quad = quads[index];

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
        BlockMesh.Quad[] quads = mesh.Quads;
        
        for (var index = 0; index < mesh.QuadCount; index++)
        {
            ref readonly BlockMesh.Quad quad = ref quads[index];
            (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = quad.data;
            
            Meshing.SetTint(ref data, mesh.Tint.Select(context.GetBlockTint(position)));
            
            meshing.PushQuadWithOffset(quad.Positions, data, offset);
        }
    }
}
