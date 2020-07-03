// <copyright file="Configuration.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System.Configuration;

namespace VoxelGame.Utilities
{
    /// <summary>
    /// Helper class to simplify configuration value retrieval.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Retrieves a float value from the configuration file.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="fallback">The fallback to use in case of failed retrieval.</param>
        /// <returns>The retrieved value.</returns>
        public static float GetFloat(string key, float fallback = default)
        {
            return float.TryParse(ConfigurationManager.AppSettings[key], out float f) ? f : fallback;
        }

        /// <summary>
        /// Retrieves an int value from the configuration file.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="fallback">The fallback to use in case of failed retrieval.</param>
        /// <returns>The retrieved value.</returns>
        public static int GetInt(string key, int fallback = default)
        {
            return int.TryParse(ConfigurationManager.AppSettings[key], out int i) ? i : fallback;
        }
    }
}