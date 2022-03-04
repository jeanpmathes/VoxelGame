// <copyright file="ApplicationInformation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core
{
    /// <summary>
    ///     Information about the current application.
    /// </summary>
    public class ApplicationInformation
    {
        private ApplicationInformation(string version)
        {
            Version = version;
        }

        /// <summary>
        ///     Information about the current game.
        /// </summary>
        public static ApplicationInformation Instance { get; private set; } = null!;

        /// <summary>
        ///     Get the game version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationInformation" /> class.
        /// </summary>
        /// <param name="version">The current application version.</param>
        public static void Initialize(string version)
        {
            Instance = new ApplicationInformation(version);
        }
    }
}
