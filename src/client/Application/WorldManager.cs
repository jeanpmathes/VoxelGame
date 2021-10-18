// <copyright file="WorldManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
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
            if (WorldActivation == null) throw new InvalidOperationException();

            ClientWorld world = new(information, path);
            WorldActivation(world);
        }

        [SuppressMessage("ReSharper", "CA2000")]
        public void CreateWorld(string name)
        {
            if (WorldActivation == null) throw new InvalidOperationException();

            StringBuilder path = new(Path.Combine(worldsDirectory, name));

            if (IsNameReserved(name)) path.Append(value: '_');

            while (Directory.Exists(path.ToString())) path.Append(value: '_');

            ClientWorld world = new(name, path.ToString(), DateTime.Now.GetHashCode());
            WorldActivation(world);
        }

        public bool IsWorldNameValid(string name)
        {
            if (name.Length == 0) return false;

            if (name[^1] == ' ') return false;

            foreach (char c in Path.GetInvalidFileNameChars())
                if (!CheckChar(c))
                    return false;

            foreach (char c in new[] {'.', ',', '{', '}'})
                if (!CheckChar(c))
                    return false;

            return true;

            bool CheckChar(char c)
            {
                return !name.Contains(c, StringComparison.Ordinal);
            }
        }

        private static bool IsNameReserved(string name)
        {
            switch (name)
            {
                case "CON":
                case "PRN":
                case "AUX":
                case "NUL":
                case "COM":
                case "COM0":
                case "COM1":
                case "COM2":
                case "COM3":
                case "COM4":
                case "COM5":
                case "COM6":
                case "COM7":
                case "COM8":
                case "COM9":
                case "LPT0":
                case "LPT1":
                case "LPT2":
                case "LPT3":
                case "LPT4":
                case "LPT5":
                case "LPT6":
                case "LPT7":
                case "LPT8":
                case "LPT9":
                    return true;

                default:
                    return false;
            }
        }

        public event Action<ClientWorld>? WorldActivation;
    }
}