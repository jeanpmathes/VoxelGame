// <copyright file="WorldManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Application
{
    public class WorldManager : IWorldProvider
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldManager>();

        private readonly List<(WorldInformation info, string path)> worlds = new();

        private readonly string worldsDirectory;

        public WorldManager(string worldsDirectory)
        {
            this.worldsDirectory = worldsDirectory;
        }

        public IEnumerable<(WorldInformation info, string path)> Worlds => worlds;

        public void Refresh()
        {
            worlds.Clear();

            foreach (string directory in Directory.GetDirectories(worldsDirectory))
            {
                string meta = Path.Combine(directory, "meta.json");

                if (File.Exists(meta))
                {
                    WorldInformation information = WorldInformation.Load(meta);
                    worlds.Add((information, directory));

                    logger.LogDebug(Events.WorldIO, "Valid world directory found: {Directory}", directory);
                }
                else
                {
                    logger.LogDebug(
                        Events.WorldIO,
                        "Directory has no meta file and is ignored: {Directory}",
                        directory);
                }
            }

            logger.LogInformation(
                Events.WorldIO,
                "Completed world lookup, found {Count} valid directories",
                worlds.Count);
        }

        [SuppressMessage("ReSharper", "CA2000")]
        public void LoadWorld(WorldInformation information, string path)
        {
            if (WorldActivation == null) return;

            ClientWorld world = new(information, path);
            WorldActivation(world);
        }

        public event Action<ClientWorld>? WorldActivation;
    }
}
