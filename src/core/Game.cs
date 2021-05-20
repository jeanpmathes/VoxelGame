// <copyright file="Game.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core
{
    public static class Game
    {
        public static string Version { get; private set; } = null!;

        public static void SetVersion(string version)
        {
            Version = version;
        }
    }
}