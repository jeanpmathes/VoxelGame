// <copyright file="BlockMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals
{
    /// <summary>
    ///     Provides information required to create a block mesh.
    /// </summary>
    public sealed class BlockMeshInfo
    {
        private BlockMeshInfo(BlockSide side, uint data, Liquid liquid)
        {
            Side = side;
            Data = data;
            Liquid = liquid;
        }

        /// <summary>
        ///     The side that is meshed.
        /// </summary>
        public BlockSide Side { get; }

        /// <summary>
        ///     The data of the block.
        /// </summary>
        public uint Data { get; }

        /// <summary>
        ///     The liquid at the block position.
        /// </summary>
        public Liquid Liquid { get; }

        /// <summary>
        ///     Mesh info for a simple block.
        /// </summary>
        public static BlockMeshInfo Simple(BlockSide side, uint data, Liquid liquid)
        {
            return new BlockMeshInfo(side, data, liquid);
        }

        /// <summary>
        ///     Mesh info for a complex block.
        /// </summary>
        public static BlockMeshInfo Complex(uint data, Liquid liquid)
        {
            return new BlockMeshInfo(BlockSide.All, data, liquid);
        }

        /// <summary>
        ///     Mesh info for a cross plant.
        /// </summary>
        public static BlockMeshInfo CrossPlant(uint data, Liquid liquid)
        {
            return new BlockMeshInfo(BlockSide.All, data, liquid);
        }

        /// <summary>
        ///     Mesh info for a crop plant.
        /// </summary>
        public static BlockMeshInfo CropPlant(uint data, Liquid liquid)
        {
            return new BlockMeshInfo(BlockSide.All, data, liquid);
        }
    }
}
