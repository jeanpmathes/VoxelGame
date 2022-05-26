// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Blocks;

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
    private readonly List<BlockMesh> meshes = new(capacity: 16);

    /// <summary>
    ///     Create a new <see cref="WideConnectingBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedId">The named ID.</param>
    /// <param name="texture">The texture to use.</param>
    /// <param name="postModel">The name of the model for the central post.</param>
    /// <param name="extensionModel">The name of the model for the connections between posts.</param>
    /// <param name="boundingVolume">The bounding box of the post.</param>
    protected WideConnectingBlock(string name, string namedId, string texture, string postModel,
        string extensionModel,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedId,
            new BlockFlags
            {
                IsFull = false,
                IsOpaque = false,
                IsSolid = true
            },
            boundingVolume,
            TargetBuffer.Complex)
    {
        BlockModel post = BlockModel.Load(postModel);
        BlockModel extension = BlockModel.Load(extensionModel);

        post.OverwriteTexture(texture);
        extension.OverwriteTexture(texture);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions =
            extension.CreateAllOrientations(rotateTopAndBottomTexture: false);

        post.Lock();
        extensions.Lock();

        List<BlockModel> requiredModels = new(capacity: 5);

        for (uint data = 0b00_0000; data <= 0b00_1111; data++)
        {
            requiredModels.Clear();
            requiredModels.Add(post);

            if ((data & 0b00_1000) != 0) requiredModels.Add(extensions.north);
            if ((data & 0b00_0100) != 0) requiredModels.Add(extensions.east);
            if ((data & 0b00_0010) != 0) requiredModels.Add(extensions.south);
            if ((data & 0b00_0001) != 0) requiredModels.Add(extensions.west);

            meshes.Add(BlockModel.GetCombinedMesh(requiredModels.ToArray()));
        }
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return GetMeshData(info);
    }

    /// <summary>
    ///     Override to change the used mesh.
    /// </summary>
    protected virtual IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return meshes[(int) info.Data & 0b00_1111].GetMeshData();
    }
}
