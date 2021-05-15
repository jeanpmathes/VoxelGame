// <copyright file="Game.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core
{
    public static class Game
    {
        #region GENRAL STATIC PROPERTIES

        public static World World { get; private set; } = null!;

        public static void SetWorld(World world)
        {
            World = world;
        }

        public static string Version { get; private set; } = null!;

        public static void SetVersion(string version)
        {
            Version = version;
        }

        public static ITextureIndexProvider BlockTextures { get; private set; } = null!;

        public static void SetBlockTextures(ITextureIndexProvider blockTextures)
        {
            BlockTextures = blockTextures;
        }

        public static ITextureIndexProvider LiquidTextures { get; private set; } = null!;

        public static void SetLiquidTextures(ITextureIndexProvider liquidTextures)
        {
            LiquidTextures = liquidTextures;
        }

        #endregion GENRAL STATIC PROPERTIES
    }
}