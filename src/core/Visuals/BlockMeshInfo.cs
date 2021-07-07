// <copyright file="BlockMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Graphics.ES11;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals
{
    public class BlockMeshInfo
    {
        public BlockSide Side { get; }
        public uint Data { get; }
        public Liquid Liquid { get; }

        private BlockMeshInfo(BlockSide side, uint data, Liquid liquid)
        {
            Side = side;
            Data = data;
            Liquid = liquid;
        }

        public static BlockMeshInfo Simple(BlockSide side, uint data, Liquid liquid) => new BlockMeshInfo(side, data, liquid);

        public static BlockMeshInfo Complex(uint data, Liquid liquid) => new BlockMeshInfo(BlockSide.All, data, liquid);

        public static BlockMeshInfo CrossPlant(uint data, Liquid liquid) => new BlockMeshInfo(BlockSide.All, data, liquid);

        public static BlockMeshInfo CropPlant(uint data, Liquid liquid) => new BlockMeshInfo(BlockSide.All, data, liquid);
    }
}