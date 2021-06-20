// <copyright file="ThinConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
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
    public class ThinConnectingBlock : Block, IThinConnectable, IFillable
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

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            (float[] vertices, int[] textureIndices, uint[] indices) = BlockModel.CombineData(out uint vertexCount, post,
                (info.Data & 0b00_1000) == 0 ? sides.north : extensions.north,
                (info.Data & 0b00_0100) == 0 ? sides.east : extensions.east,
                (info.Data & 0b00_0010) == 0 ? sides.south : extensions.south,
                (info.Data & 0b00_0001) == 0 ? sides.west : extensions.west);

            return new BlockMeshData(vertexCount, vertices, textureIndices, indices);
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            world.SetBlock(this, IConnectable.GetConnectionData<IThinConnectable>(world, x, y, z), x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            uint newData = data;

            newData = side switch
            {
                BlockSide.Back => CheckNeighbor(x, y, z - 1, BlockSide.Front, 0b00_1000, newData),
                BlockSide.Right => CheckNeighbor(x + 1, y, z, BlockSide.Left, 0b00_0100, newData),
                BlockSide.Front => CheckNeighbor(x, y, z + 1, BlockSide.Back, 0b00_0010, newData),
                BlockSide.Left => CheckNeighbor(x - 1, y, z, BlockSide.Right, 0b00_0001, newData),
                _ => newData
            };

            if (newData != data)
            {
                world.SetBlock(this, newData, x, y, z);
            }

            uint CheckNeighbor(int nx, int ny, int nz, BlockSide neighborSide, uint mask, uint oldData)
            {
                if (world.GetBlock(nx, ny, nz, out _) is IThinConnectable neighbor && neighbor.IsConnectable(world, neighborSide, nx, ny, nz))
                {
                    oldData |= mask;
                }
                else
                {
                    oldData &= ~mask;
                }

                return oldData;
            }
        }
    }
}