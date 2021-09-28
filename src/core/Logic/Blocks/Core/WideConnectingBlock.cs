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

        private protected readonly string texture;

        protected WideConnectingBlock(string name, string namedId, string texture, string post, string extension,
            BoundingBox boundingBox) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.texture = texture;

            BlockModel postModel = BlockModel.Load(post);
            BlockModel extensionModel = BlockModel.Load(extension);

            postModel.OverwriteTexture(texture);
            extensionModel.OverwriteTexture(texture);

            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensionModels =
                extensionModel.CreateAllOrientations(rotateTopAndBottomTexture: false);

            postModel.Lock();
            extensionModels.Lock();

            List<BlockModel> requiredModels = new(capacity: 5);

            for (uint data = 0b00_0000; data <= 0b00_1111; data++)
            {
                requiredModels.Clear();
                requiredModels.Add(postModel);

                if ((data & 0b00_1000) != 0) requiredModels.Add(extensionModels.north);
                if ((data & 0b00_0100) != 0) requiredModels.Add(extensionModels.east);
                if ((data & 0b00_0010) != 0) requiredModels.Add(extensionModels.south);
                if ((data & 0b00_0001) != 0) requiredModels.Add(extensionModels.west);

                meshes.Add(BlockModel.GetCombinedMesh(requiredModels.ToArray()));
            }
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return meshes[(int) info.Data & 0b00_1111].GetComplexMeshData();
        }
    }
}