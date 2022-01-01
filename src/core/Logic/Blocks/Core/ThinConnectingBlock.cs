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
    ///     A thin block that connects to other blocks.
    ///     Data bit usage: <c>--nesw</c>
    /// </summary>
    // n: connected north
    // e: connected east
    // s: connected south
    // w: connected west
    public class ThinConnectingBlock : ConnectingBlock<IThinConnectable>, IThinConnectable
    {
        private readonly List<BlockMesh> meshes = new(capacity: 16);

        /// <inheritdoc />
        public ThinConnectingBlock(string name, string namedId, string postModel, string sideModel,
            string extensionModel) :
            base(
                name,
                namedId,
                new BlockFlags
                {
                    IsSolid = true
                },
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.0625f, y: 0.5f, z: 0.0625f)),
                TargetBuffer.Complex)
        {
            BlockModel post = BlockModel.Load(postModel);

            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) sides =
                BlockModel.Load(sideModel).CreateAllOrientations(rotateTopAndBottomTexture: false);

            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions =
                BlockModel.Load(extensionModel).CreateAllOrientations(rotateTopAndBottomTexture: false);

            for (uint data = 0b00_0000; data <= 0b00_1111; data++)
            {
                BlockMesh mesh = BlockModel.GetCombinedMesh(
                    post,
                    (data & 0b00_1000) == 0 ? sides.north : extensions.north,
                    (data & 0b00_0100) == 0 ? sides.east : extensions.east,
                    (data & 0b00_0010) == 0 ? sides.south : extensions.south,
                    (data & 0b00_0001) == 0 ? sides.west : extensions.west);

                meshes.Add(mesh);
            }
        }

        /// <inheritdoc />
        protected override BoundingBox GetBoundingBox(uint data)
        {
            List<BoundingBox> connectors = new(BitHelper.CountSetBits(data));

            if ((data & 0b00_1000) != 0)
                connectors.Add(
                    new BoundingBox(
                        new Vector3(x: 0.5f, y: 0.5f, z: 0.21875f),
                        new Vector3(x: 0.0625f, y: 0.5f, z: 0.21875f)));

            if ((data & 0b00_0100) != 0)
                connectors.Add(
                    new BoundingBox(
                        new Vector3(x: 0.78125f, y: 0.5f, z: 0.5f),
                        new Vector3(x: 0.21875f, y: 0.5f, z: 0.0625f)));

            if ((data & 0b00_0010) != 0)
                connectors.Add(
                    new BoundingBox(
                        new Vector3(x: 0.5f, y: 0.5f, z: 0.78125f),
                        new Vector3(x: 0.0625f, y: 0.5f, z: 0.21875f)));

            if ((data & 0b00_0001) != 0)
                connectors.Add(
                    new BoundingBox(
                        new Vector3(x: 0.21875f, y: 0.5f, z: 0.5f),
                        new Vector3(x: 0.21875f, y: 0.5f, z: 0.0625f)));

            return new BoundingBox(
                new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3(x: 0.0625f, y: 0.5f, z: 0.0625f),
                connectors.ToArray());
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMesh mesh = meshes[(int) info.Data & 0b00_1111];

            return mesh.GetComplexMeshData();
        }
    }
}