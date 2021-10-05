// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A base class for blocks that connect to other blocks as wide connectables, like fences or walls.
    ///     Data bit usage: <c>--nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public abstract class WideConnectingBlock : ConnectingBlock<IWideConnectable>, IWideConnectable
    {
        private readonly List<BlockMesh> meshes = new(capacity: 16);

        protected WideConnectingBlock(string name, string namedId, string texture, string postModel,
            string extensionModel,
            BoundingBox boundingBox) :
            base(
                name,
                namedId,
                new BlockFlags
                {
                    IsFull = false,
                    IsOpaque = false,
                    IsSolid = true
                },
                boundingBox,
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

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return meshes[(int) info.Data & 0b00_1111].GetComplexMeshData();
        }
    }
}