// <copyright file="WorldProvider.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Application;

/// <summary>
///     Provides worlds that are either loaded from disk or newly created.
///     The world provider itself does not active worlds.
/// </summary>
public class WorldProvider : IWorldProvider
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldProvider>();

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

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void LoadWorld(WorldInformation information, string path)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        ClientWorld world = new(information, path);
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

        ClientWorld world = new(name, path.ToString(), DateTime.Now.GetHashCode());
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

    /// <summary>
    ///     Is invoked when a world is requested to be activated.
    /// </summary>
    public event EventHandler<ClientWorld> WorldActivation = null!;
}
