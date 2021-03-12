// <copyright file="LiquidMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals
{
    public class LiquidMeshInfo
    {
        public LiquidLevel Level { get; }
        public BlockSide Side { get; }
        public bool IsStatic { get; }

        public LiquidMeshInfo(LiquidLevel level, BlockSide side, bool isStatic)
        {
            Level = level;
            Side = side;
            IsStatic = isStatic;
        }
    }
}