// <copyright file="FoliageBlock.cs" company="VoxelGame">
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
///     Blocks that use specialized foliage meshing.
/// </summary>
public class FoliageBlock : Block
{
    private static readonly BlockMesh.Quad[] errorQuads;
    private static readonly UInt32 errorQuadCount;

    static FoliageBlock()
    {
        errorQuads = BlockMeshes.CreateCrossMesh(ITextureIndexProvider.MissingTextureIndex).GetMeshData(out errorQuadCount);
    }
    
    private readonly Foliage foliage;

    private Foliage.MeshData[] meshData = null!;
    
    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Foliage;

    /// <inheritdoc />
    public FoliageBlock(String name, UInt32 id, String namedID) : base(name, id, namedID)
    {
        foliage = Require<Foliage>();
    }

    /// <inheritdoc />
    protected override void OnValidate()
    {
        
    }

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals)
    {
        meshData = new Foliage.MeshData[States.Count];
        
        foreach ((State state, Int32 index) in States.GetAllStatesWithIndex())
        {
            if (!Constraint.IsStateValid(state))
            {
                meshData[index] = new Foliage.MeshData(errorQuads, errorQuadCount, ColorS.None, Foliage.PartType.Single, IsAnimated: false);
                continue;
            }

            Foliage.MeshData mesh = foliage.GetMeshData(state, textureIndexProvider, visuals);
            BuildMeshData(mesh);
            meshData[index] = mesh;
        }
    }

    private void BuildMeshData(Foliage.MeshData mesh)
    {
        BlockMesh.Quad[] quads = mesh.Quads;
        
        for (var index = 0; index < mesh.QuadCount; index++)
        {
            ref BlockMesh.Quad quad = ref quads[index];
            
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
        
        ref readonly Foliage.MeshData mesh = ref meshData[state.Index]; // todo: use ref readonly in the other Mesh overrides as well
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
