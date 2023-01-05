// <copyright file="WorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Application;

/// <summary>
///     Provides worlds that are either loaded from disk or newly created.
///     The world provider itself does not active worlds.
/// </summary>
public class WorldProvider : IWorldProvider
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldProvider>();

    private static readonly ISet<string> reservedNames = new HashSet<string>
    {
        "CON",
        "PRN",
        "AUX",
        "NUL",
        "COM",
        "COM0",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT0",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9"
    };

    private readonly List<(WorldInformation info, string path)> worlds = new();

    private readonly string worldsDirectory;

    /// <summary>
    ///     Create a new world provider.
    /// </summary>
    /// <param name="worldsDirectory">The directory where worlds are loaded from and saved to.</param>
    public WorldProvider(string worldsDirectory)
    {
        this.worldsDirectory = worldsDirectory;
    }

    /// <inheritdoc />
    public IEnumerable<(WorldInformation info, string path)> Worlds => worlds;

    /// <inheritdoc />
    public void Refresh()
    {
        worlds.Clear();

        foreach (string directory in Directory.GetDirectories(worldsDirectory))
        {
            if (WorldData.IsWorldDirectory(directory))
            {
                WorldInformation information = WorldData.LoadInformation(directory);
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

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void LoadWorld(WorldInformation information, string path)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        ClientWorld world = new(path, information);
        WorldActivation(this, world);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void CreateWorld(string name)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        StringBuilder path = new(Path.Combine(worldsDirectory, name));

        if (IsNameReserved(name)) path.Append(value: '_');

        while (Directory.Exists(path.ToString())) path.Append(value: '_');

        (int upper, int lower) seed = (DateTime.Now.GetHashCode(), RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue));

        ClientWorld world = new(path.ToString(), name, seed);
        WorldActivation(world, world);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void DeleteWorld(string path)
    {
        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException e)
        {
            logger.LogError(Events.FileIO, e, "Failed to delete world");
        }
    }

    private static bool IsNameReserved(string name)
    {
        return reservedNames.Contains(name);
    }

    /// <summary>
    ///     Is invoked when a world is requested to be activated.
    /// </summary>
    public event EventHandler<ClientWorld> WorldActivation = null!;
}
