// <copyright file="LiquidMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals
{
    /// <summary>
    ///     Information required to mesh a liquid.
    /// </summary>
    public sealed class LiquidMeshInfo
    {
        private LiquidMeshInfo(LiquidLevel level, BlockSide side, bool isStatic)
        {
            Level = level;
            Side = side;
            IsStatic = isStatic;
        }

        /// <summary>
        ///     The level of the liquid.
        /// </summary>
        public LiquidLevel Level { get; }

        /// <summary>
        ///     The side of the liquid that is being meshed.
        /// </summary>
        public BlockSide Side { get; }

        /// <summary>
        ///     Whether the liquid is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        ///     Create liquid meshing information.
        /// </summary>
        public static LiquidMeshInfo Liquid(LiquidLevel level, BlockSide side, bool isStatic)
        {
            return new(level, side, isStatic);
        }
    }
}
