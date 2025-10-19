// <copyright file="FoliageBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    private static readonly Mesh.Quad[] errorQuads;
    private static readonly UInt32 errorQuadCount;

    private readonly Foliage foliage;

    private Foliage.MeshData[] meshData = null!;

    static FoliageBlock()
    {
        errorQuads = Meshes.CreateCrossMesh(ITextureIndexProvider.MissingTextureIndex).GetMeshData(out errorQuadCount);
    }

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
}
