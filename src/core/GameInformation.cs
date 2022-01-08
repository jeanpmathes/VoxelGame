// <copyright file="Game.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core
{
    /// <summary>
    ///     Information about a running game.
    /// </summary>
    public class GameInformation
    {
        private GameInformation(string version)
        {
            Version = version;
        }

        /// <summary>
        ///     Information about the current game.
        /// </summary>
        public static GameInformation Instance { get; private set; } = null!;

        /// <summary>
        ///     Get the game version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameInformation" /> class.
        /// </summary>
        /// <param name="version">The active game version.</param>
        public static void Initialize(string version)
        {
            Instance = new GameInformation(version);
        }
    }
}