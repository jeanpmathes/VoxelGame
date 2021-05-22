// <copyright file="Game.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core
{
    public class GameInformation
    {
        public static GameInformation Instance { get; private set; } = null!;

        public static void Initialize(string version)
        {
            Instance = new GameInformation(version);
        }

        public string Version { get; }

        private GameInformation(string version)
        {
            Version = version;
        }
    }
}