// <copyright file="WorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using World = VoxelGame.Client.Logic.World;

namespace VoxelGame.Client.Application;

/// <summary>
///     Provides worlds that are either loaded from disk or newly created.
///     The world provider itself does not active worlds.
/// </summary>
public class WorldProvider : IWorldProvider
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldProvider>();

    private readonly List<(WorldInformation info, DirectoryInfo path)> worlds = new();

    private readonly DirectoryInfo worldsDirectory;

    /// <summary>
    ///     Create a new world provider.
    /// </summary>
    /// <param name="worldsDirectory">The directory where worlds are loaded from and saved to.</param>
    public WorldProvider(DirectoryInfo worldsDirectory)
    {
        this.worldsDirectory = worldsDirectory;
    }

    /// <inheritdoc />
    public IEnumerable<(WorldInformation info, DirectoryInfo path)> Worlds => worlds;

    /// <inheritdoc />
    public void Refresh()
    {
        worlds.Clear();

        foreach (DirectoryInfo directory in worldsDirectory.EnumerateDirectories())
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

        logger.LogInformation(
            Events.WorldIO,
            "Completed world lookup, found {Count} valid directories",
            worlds.Count);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void LoadWorld(WorldInformation information, DirectoryInfo path)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        World world = new(path, information);
        WorldActivation(this, world);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void CreateWorld(string name)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        (int upper, int lower) seed = (DateTime.Now.GetHashCode(), RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue));

        DirectoryInfo worldDirectory = FileSystem.GetUniqueDirectory(worldsDirectory, name);

        World world = new(worldDirectory, name, seed);
        WorldActivation(world, world);
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public bool IsWorldNameValid(string name)
    {
        if (name.Length == 0) return false;

        if (name[^1] == ' ') return false;

        if (Path.GetInvalidFileNameChars().Any(c => !IsCharInName(c))) return false;

        if (new[] {'.', ',', '{', '}'}.Any(c => !IsCharInName(c))) return false;

        return true;

        bool IsCharInName(char c)
        {
            return !name.Contains(c, StringComparison.Ordinal);
        }
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public void DeleteWorld(DirectoryInfo path)
    {
        try
        {
            path.Delete(recursive: true);
        }
        catch (IOException e)
        {
            logger.LogError(Events.FileIO, e, "Failed to delete world");
        }
    }

    /// <summary>
    ///     Is invoked when a world is requested to be activated.
    /// </summary>
    public event EventHandler<World> WorldActivation = null!;
}
