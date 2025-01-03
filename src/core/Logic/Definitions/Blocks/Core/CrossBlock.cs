﻿// <copyright file="CrossBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;
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
    private readonly TID texture;

    private BlockMesh mesh = null!;

    /// <summary>
    ///     Initializes a new instance of a cross block; a block made out of two intersecting planes.
    ///     Cross blocks are never full, solid, or opaque.
    /// </summary>
    protected CrossBlock(String name, String namedID, TID texture, BlockFlags flags,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            flags with {IsFull = false, IsOpaque = false, IsSolid = false},
            boundingVolume)
    {
        this.texture = texture;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return mesh.GetMeshData();
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        mesh = BlockMeshes.CreateCrossMesh(textureIndexProvider.GetTextureIndex(texture));
    }
}
