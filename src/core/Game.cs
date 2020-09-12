using System;
using System.Diagnostics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core
{
    public class Game
    {
        #region GENRAL STATIC PROPERTIES

        public static Player Player { get; private set; } = null!;

        public static void SetPlayer(Player player)
        {
            Debug.Assert(Player == null);
            Player = player;
        }

        public static World World { get; private set; } = null!;

        public static void SetWorld(World world)
        {
            Debug.Assert(World == null);
            World = world;
        }

        public static Random Random { get; private set; } = null!;

        public static void SetRandom(Random random)
        {
            Debug.Assert(Random == null);
            Random = random;
        }

        public static string Version { get; private set; } = null!;

        public static void SetVersion(string version)
        {
            Debug.Assert(Version == null);
            Version = version;
        }

        public static ITextureIndexProvider BlockTextures { get; private set; } = null!;

        public static void SetBlockTextures(ITextureIndexProvider blockTextures)
        {
            Debug.Assert(BlockTextures == null);
            BlockTextures = blockTextures;
        }

        public static ITextureIndexProvider LiquidTextures { get; private set; } = null!;

        public static void SetLiquidTextures(ITextureIndexProvider liquidTextures)
        {
            Debug.Assert(LiquidTextures == null);
            LiquidTextures = liquidTextures;
        }

        #endregion GENRAL STATIC PROPERTIES

        #region TICK MANAGMENT

        /// <summary>
        /// The number of the current update cycle. It is incremented every time a new cycle begins.
        /// </summary>
        public static long CurrentUpdate { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Limit access.")]
        public void Update(float deltaTime)
        {
            CurrentUpdate++;

            World.Update(deltaTime);
        }

        #endregion TICK MANAGMENT
    }
}