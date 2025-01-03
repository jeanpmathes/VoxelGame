// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A base class for blocks that connect to other blocks as wide connectables, like fences or walls.
///     Data bit usage: <c>--nesw</c>
/// </summary>
// n: connected north
// e: connected east
// s: connected south
// w: connected west
public class WideConnectingBlock : ConnectingBlock<IWideConnectable>, IWideConnectable, IComplex
{
    private readonly TID texture;
    private readonly RID postModel;
    private readonly RID extensionModel;

    private readonly List<BlockMesh> meshes = new(capacity: 16);

    /// <summary>
    ///     Create a new <see cref="WideConnectingBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The named ID.</param>
    /// <param name="texture">The texture to use.</param>
    /// <param name="isOpaque">Whether the block is opaque.</param>
    /// <param name="postModel">The name of the model for the central post.</param>
    /// <param name="extensionModel">The name of the model for the connections between posts.</param>
    /// <param name="boundingVolume">The bounding box of the post.</param>
    protected WideConnectingBlock(
        String name,
        String namedID,
        TID texture,
        Boolean isOpaque,
        RID postModel,
        RID extensionModel,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            new BlockFlags
            {
                IsOpaque = isOpaque,
                IsSolid = true
            },
            boundingVolume)
    {
        this.texture = texture;
        this.postModel = postModel;
        this.extensionModel = extensionModel;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return GetMeshData(info);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        BlockModel post = modelProvider.GetModel(postModel);
        BlockModel extension = modelProvider.GetModel(extensionModel);

        post.OverwriteTexture(texture);
        extension.OverwriteTexture(texture);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions =
            extension.CreateAllOrientations(rotateTopAndBottomTexture: false);

        post.Lock(textureIndexProvider);
        extensions.Lock(textureIndexProvider);

        List<BlockModel> requiredModels = new(capacity: 5);

        for (UInt32 data = 0b00_0000; data <= 0b00_1111; data++)
        {
            requiredModels.Clear();
            requiredModels.Add(post);

            if ((data & 0b00_1000) != 0) requiredModels.Add(extensions.north);
            if ((data & 0b00_0100) != 0) requiredModels.Add(extensions.east);
            if ((data & 0b00_0010) != 0) requiredModels.Add(extensions.south);
            if ((data & 0b00_0001) != 0) requiredModels.Add(extensions.west);

            meshes.Add(BlockModel.GetCombinedMesh(textureIndexProvider, requiredModels.ToArray()));
        }
    }

    /// <summary>
    ///     Override to change the used mesh.
    /// </summary>
    protected virtual IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return meshes[(Int32) info.Data & 0b00_1111].GetMeshData();
    }
}
