using System;
using System.Diagnostics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core
{
    public static class Game
    {
        #region GENRAL STATIC PROPERTIES

        public static Player Player { get; private set; } = null!;

        public static void SetPlayer(Player player)
        {
            Player = player;
        }

        public static World World { get; private set; } = null!;

        public static void SetWorld(World world)
        {
            World = world;
        }

        public static Random Random { get; private set; } = null!;

        public static void SetRandom(Random random)
        {
            Random = random;
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

        #region TICK MANAGMENT

        /// <summary>
        /// The number of the current update cycle. It is incremented every time a new cycle begins.
        /// </summary>
        public static long CurrentUpdate { get; private set; }

        public static void IncrementUpdate()
        {
            CurrentUpdate++;
        }

        public static void ResetUpdate()
        {
            CurrentUpdate = 0;
        }

        #endregion TICK MANAGMENT
    }
}