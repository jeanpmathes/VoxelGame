// <copyright file="WorldInformation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     Basic information about a world.
    /// </summary>
    public class WorldInformation
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldInformation>();

        /// <summary>
        ///     The name of the world.
        /// </summary>
        public string Name { get; set; } = "No Name";

        /// <summary>
        ///     The seed of the world.
        /// </summary>
        public int Seed { get; set; } = 2133;

        /// <summary>
        ///     The creation date of the world.
        /// </summary>
        public DateTime Creation { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The game version in which the world was created.
        /// </summary>
        public string Version { get; set; } = "missing";

        /// <summary>
        ///     The spawn information of the world.
        /// </summary>
        public SpawnInformation SpawnInformation { get; set; } = new(new Vector3(x: 0f, y: 1024f, z: 0f));

        /// <summary>
        ///     Save this world information to a file.
        /// </summary>
        /// <param name="path">The save path.</param>
        public void Save(string path)
        {
            JsonSerializerOptions options = new()
            {
                IgnoreReadOnlyProperties = true,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        ///     Load a world information from a file. If loading fails, a default world information is returned.
        /// </summary>
        /// <param name="path">The path to load from.</param>
        /// <returns>The loaded world information.</returns>
        public static WorldInformation Load(string path)
        {
            try
            {
                string json = File.ReadAllText(path);

                WorldInformation information =
                    JsonSerializer.Deserialize<WorldInformation>(json) ?? new WorldInformation();

                logger.LogDebug(
                    Events.WorldIO,
                    "WorldInformation for World '{Name}' was loaded from: {Path}",
                    information.Name,
                    path);

                return information;
            }
            catch (JsonException exception)
            {
                logger.LogError(Events.WorldLoadingError, exception, "The meta file could not be loaded: {Path}", path);

                return new WorldInformation();
            }
        }
    }

    /// <summary>
    ///     World spawn information.
    /// </summary>
    public struct SpawnInformation : IEquatable<SpawnInformation>
    {
        /// <summary>
        ///     The x position of the spawn.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        ///     The y position of the spawn.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        ///     The z position of the spawn.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        ///     Create spawn information from a vector.
        /// </summary>
        /// <param name="position">The position.</param>
        public SpawnInformation(Vector3 position)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
        }

        /// <summary>
        ///     Get the position of the spawn information.
        /// </summary>
        public Vector3 Position => new(X, Y, Z);

        /// <inheritdoc />
        public bool Equals(SpawnInformation other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is SpawnInformation other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        ///     Determine equality between two spawn information.
        /// </summary>
        public static bool operator ==(SpawnInformation left, SpawnInformation right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Determine inequality between two spawn information.
        /// </summary>
        public static bool operator !=(SpawnInformation left, SpawnInformation right)
        {
            return !left.Equals(right);
        }
    }
}
