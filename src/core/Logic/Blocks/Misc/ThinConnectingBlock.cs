// <copyright file="ThinConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A thin block that connects to other blocks.
    /// Data bit usage: <c>--nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public class ThinConnectingBlock : ConnectingBlock<IThinConnectable>, IThinConnectable
    {
        private readonly BlockModel post;
        private readonly (BlockModel north, BlockModel east, BlockModel south, BlockModel west) sides;
        private readonly (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions;

        public ThinConnectingBlock(string name, string namedId, string postModel, string sideModel, string extensionModel) :
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
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0625f, 0.5f, 0.0625f)),
                TargetBuffer.Complex)
        {
            post = BlockModel.Load(postModel);
            sides = BlockModel.Load(sideModel).CreateAllDirections(false);
            extensions = BlockModel.Load(extensionModel).CreateAllDirections(false);
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            List<BoundingBox> connectors = new List<BoundingBox>(BitHelper.CountSetBits(data));

            if ((data & 0b00_1000) != 0) connectors.Add(new BoundingBox(new Vector3(0.5f, 0.5f, 0.21875f), new Vector3(0.0625f, 0.5f, 0.21875f)));
            if ((data & 0b00_0100) != 0) connectors.Add(new BoundingBox(new Vector3(0.78125f, 0.5f, 0.5f), new Vector3(0.21875f, 0.5f, 0.0625f)));
            if ((data & 0b00_0010) != 0) connectors.Add(new BoundingBox(new Vector3(0.5f, 0.5f, 0.78125f), new Vector3(0.0625f, 0.5f, 0.21875f)));
            if ((data & 0b00_0001) != 0) connectors.Add(new BoundingBox(new Vector3(0.21875f, 0.5f, 0.5f), new Vector3(0.21875f, 0.5f, 0.0625f)));

            return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0625f, 0.5f, 0.0625f), connectors.ToArray());
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            (float[] vertices, int[] textureIndices, uint[] indices) = BlockModel.CombineData(out uint vertexCount, post,
                (info.Data & 0b00_1000) == 0 ? sides.north : extensions.north,
                (info.Data & 0b00_0100) == 0 ? sides.east : extensions.east,
                (info.Data & 0b00_0010) == 0 ? sides.south : extensions.south,
                (info.Data & 0b00_0001) == 0 ? sides.west : extensions.west);

            return BlockMeshData.Complex(vertexCount, vertices, textureIndices, indices);
        }
    }
}