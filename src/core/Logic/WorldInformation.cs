﻿// <copyright file="WorldInformation.cs" company="VoxelGame">
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
    public class WorldInformation
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldInformation>();

        public string Name { get; set; } = "No Name";
        public int Seed { get; set; } = 2133;
        public DateTime Creation { get; set; } = DateTime.MinValue;
        public string Version { get; set; } = "missing";
        public SpawnInformation SpawnInformation { get; set; } = new(new Vector3(x: 0f, y: 1024f, z: 0f));

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

        public static WorldInformation Load(string path)
        {
            try
            {
                string json = File.ReadAllText(path);

                WorldInformation information =
                    JsonSerializer.Deserialize<WorldInformation>(json) ?? new WorldInformation();

                logger.LogDebug("WorldInformation for World '{Name}' was loaded from: {Path}", information.Name, path);

                return information;
            }
            catch (JsonException exception)
            {
                logger.LogError(Events.WorldLoadingError, exception, "The meta file could not be loaded: {Path}", path);

                return new WorldInformation();
            }
        }
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types

    public struct SpawnInformation
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public SpawnInformation(Vector3 position)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
        }

        public Vector3 Position => new(X, Y, Z);
    }
}