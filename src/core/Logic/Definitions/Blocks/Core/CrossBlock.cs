// <copyright file="CrossBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block with two crossed quads.
///     Data bit usage: <c>------</c>
/// </summary>
public class CrossBlock : Block, IFillable, IComplex
{
    private readonly string texture;

    private uint[] indices = null!;
    private int[] textureIndices = null!;
    private float[] vertices = null!;

    /// <summary>
    ///     Initializes a new instance of a cross block; a block made out of two intersecting planes.
    ///     Cross blocks are never full, solid, or opaque.
    /// </summary>
    protected CrossBlock(string name, string namedId, string texture, BlockFlags flags,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedId,
            flags with {IsFull = false, IsOpaque = false, IsSolid = false},
            boundingVolume)
    {
        this.texture = texture;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return IComplex.CreateData(vertexCount: 8, vertices, textureIndices, indices);
    }

    /// <inheritdoc />
    protected override void Setup(ITextureIndexProvider indexProvider)
    {
        (vertices, indices, textureIndices) = BlockModels.CreateCrossModel(indexProvider.GetTextureIndex(texture));
    }
}

