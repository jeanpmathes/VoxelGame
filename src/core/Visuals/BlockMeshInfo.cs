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

        public BlockMeshInfo(BlockSide side, uint data, Liquid liquid)
        {
            Side = side;
            Data = data;
            Liquid = liquid;
        }
    }
}