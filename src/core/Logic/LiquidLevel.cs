// <copyright file="LiquidLevel.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core.Logic
{
    public enum LiquidLevel
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3,
        Five = 4,
        Six = 5,
        Seven = 6,
        Eight = 7
    }

    public static class LiquidLevelExtensions
    {
        public static int GetBlockHeight(this LiquidLevel level)
        {
            return ((int)level * 2) + 1;
        }
    }
}