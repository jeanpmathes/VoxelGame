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
    }
}