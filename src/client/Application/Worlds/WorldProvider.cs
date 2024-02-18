// <copyright file="WorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using World = VoxelGame.Client.Logic.World;

namespace VoxelGame.Client.Application.Worlds;

/// <summary>
///     Provides worlds that are either loaded from disk or newly created.
///     The world provider itself does not active worlds.
/// </summary>
public class WorldProvider : IWorldProvider
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldProvider>();

    private readonly DirectoryInfo worldsDirectory;
    private readonly FileInfo metadataFile;

    private readonly List<WorldData> worlds = new();
    private WorldDirectoryMetadata metadata = new();

    /// <summary>
    ///     Create a new world provider.
    /// </summary>
    /// <param name="worldsDirectory">The directory where worlds are loaded from and saved to.</param>
    public WorldProvider(DirectoryInfo worldsDirectory)
    {
        this.worldsDirectory = worldsDirectory;

        metadataFile = worldsDirectory.GetFile("meta.json");
    }

    private Status Status { get; set; } = Status.Ok;

    /// <inheritdoc />
    public IEnumerable<WorldData> Worlds
    {
        get
        {
            if (Status != Status.Ok) throw new InvalidOperationException();

            return worlds;
        }
    }

    /// <inheritdoc />
    public DateTime? GetDateTimeOfLastLoad(WorldData data)
    {
        if (Status != Status.Ok) throw new InvalidOperationException();

        metadata.Entries.TryGetValue(GetMetadataKey(data), out WorldFileMetadata? fileMetadata);

        return fileMetadata?.LastLoad;
    }

    /// <inheritdoc />
    public Operation Refresh()
    {
        if (Status == Status.Running)
            throw new InvalidOperationException();

        Status = Status.Running;

        worlds.Clear();

        return Operations.Launch(() =>
        {
            WorldDirectoryMetadata loaded = WorldDirectoryMetadata.Load(metadataFile, out Exception? exception);

            if (exception != null)
                throw exception;

            metadata = loaded;

            List<WorldData>? found;

            try
            {
                found = SearchForWorlds();

                logger.LogInformation(
                    Events.WorldIO,
                    "Completed world lookup, found {Count} valid directories",
                    found.Count);
            }
            catch (Exception e) when (e is IOException or SecurityException)
            {
                logger.LogError(Events.WorldIO, e, "Failed to refresh worlds");

                throw;
            }

            return found;
        }).OnCompletion(op =>
        {
            if (op.Result != null)
                worlds.AddRange(op.Result);

            Status = op.Status;
        });
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void BeginLoadingWorld(WorldData data)
    {
        if (WorldActivation == null) throw new InvalidOperationException();
        if (Status != Status.Ok) throw new InvalidOperationException();

        World world = new(data);
        ActivateWorld(world);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void BeginCreatingWorld(string name)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        (int upper, int lower) seed = (DateTime.UtcNow.GetHashCode(), RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue));

        DirectoryInfo worldDirectory = FileSystem.GetUniqueDirectory(worldsDirectory, name);

        World world = new(worldDirectory, name, seed);
        ActivateWorld(world);
    }

    /// <inheritdoc />
    public Operation DeleteWorld(WorldData data)
    {
        if (Status != Status.Ok) throw new InvalidOperationException();

        worlds.Remove(data);
        metadata.Entries.Remove(GetMetadataKey(data));

        return data.Delete();
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public bool IsWorldNameValid(string name)
    {
        if (name.Length == 0) return false;

        if (name[^1] == ' ') return false;

        if (Array.Exists(Path.GetInvalidFileNameChars(), c => !IsCharInName(c))) return false;
        if (Array.Exists(new[] {'.', ',', '{', '}'}, c => !IsCharInName(c))) return false;

        return true;

        bool IsCharInName(char c)
        {
            return !name.Contains(c, StringComparison.Ordinal);
        }
    }

    private List<WorldData> SearchForWorlds()
    {
        List<WorldData> found = new();

        foreach (DirectoryInfo directory in worldsDirectory.EnumerateDirectories())
            if (WorldData.IsWorldDirectory(directory))
            {
                found.Add(WorldData.LoadInformation(directory));

                logger.LogDebug(Events.WorldIO, "Valid world directory found: {Directory}", directory);
            }
            else
            {
                logger.LogDebug(
                    Events.WorldIO,
                    "Directory has no meta file and is ignored: {Directory}",
                    directory);
            }

        return found;
    }

    private void ActivateWorld(World world)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        metadata.Entries[GetMetadataKey(world.Data)] = new WorldFileMetadata
        {
            LastLoad = DateTime.UtcNow
        };

        metadata.Save(metadataFile);

        WorldActivation(this, world);
    }

    private static string GetMetadataKey(WorldData data)
    {
        return data.WorldDirectory.Name;
    }

    /// <summary>
    ///     Is invoked when a world is requested to be activated.
    /// </summary>
    public event EventHandler<World> WorldActivation = null!;
}
