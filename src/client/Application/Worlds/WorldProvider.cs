﻿// <copyright file="WorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections.Properties;
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

    private readonly FileInfo metadataFile;

    private readonly List<IWorldProvider.IWorldInfo> worlds = new();
    private WorldDirectoryMetadata metadata = new();

    /// <summary>
    ///     Create a new world provider.
    /// </summary>
    /// <param name="worldsDirectory">The directory where worlds are loaded from and saved to.</param>
    public WorldProvider(DirectoryInfo worldsDirectory)
    {
        WorldsDirectory = worldsDirectory;

        metadataFile = worldsDirectory.GetFile("meta.json");
    }

    private Status Status { get; set; } = Status.Ok;

    /// <inheritdoc />
    public DirectoryInfo WorldsDirectory { get; }

    /// <inheritdoc />
    public IEnumerable<IWorldProvider.IWorldInfo> Worlds
    {
        get
        {
            if (Status != Status.Ok) throw new InvalidOperationException();

            return worlds;
        }
    }

    /// <inheritdoc />
    public Operation<Property> GetWorldProperties(IWorldProvider.IWorldInfo info)
    {
        return Operations.Launch(GetData(info).DetermineProperties);
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
                worlds.AddRange(op.Result.Select(data => new WorldInfo(data, this)));

            Status = op.Status;
        });
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void BeginLoadingWorld(IWorldProvider.IWorldInfo info)
    {
        if (WorldActivation == null) throw new InvalidOperationException();
        if (Status != Status.Ok) throw new InvalidOperationException();

        World world = new(GetData(info));
        ActivateWorld(world);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "CA2000")]
    public void BeginCreatingWorld(string name)
    {
        if (WorldActivation == null) throw new InvalidOperationException();

        (int upper, int lower) seed = (DateTime.UtcNow.GetHashCode(), RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue));

        DirectoryInfo worldDirectory = FileSystem.GetUniqueDirectory(WorldsDirectory, name);

        World world = new(worldDirectory, name, seed);
        ActivateWorld(world);
    }

    /// <inheritdoc />
    public Operation DeleteWorld(IWorldProvider.IWorldInfo info)
    {
        if (Status != Status.Ok) throw new InvalidOperationException();

        WorldData data = GetData(info);

        worlds.Remove(info);
        metadata.Entries.Remove(GetMetadataKey(data));

        return data.Delete();
    }

    /// <inheritdoc />
    public void RenameWorld(IWorldProvider.IWorldInfo info, string newName)
    {
        if (Status != Status.Ok) throw new InvalidOperationException();

        GetData(info).Rename(newName);
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public bool IsWorldNameValid(string name)
    {
        string valid = WorldData.MakeWorldNameValid(name);

        return valid == name;
    }

    private DateTime? GetDateTimeOfLastLoad(WorldData data)
    {
        if (Status != Status.Ok) throw new InvalidOperationException();

        metadata.Entries.TryGetValue(GetMetadataKey(data), out WorldFileMetadata? fileMetadata);

        return fileMetadata?.LastLoad;
    }

    private List<WorldData> SearchForWorlds()
    {
        List<WorldData> found = new();

        foreach (DirectoryInfo directory in WorldsDirectory.EnumerateDirectories())
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

    private static WorldData GetData(IWorldProvider.IWorldInfo info)
    {
        if (info is WorldInfo worldInfo)
            return worldInfo.Data;

        throw new InvalidOperationException();
    }

    /// <summary>
    ///     Is invoked when a world is requested to be activated.
    /// </summary>
    public event EventHandler<World> WorldActivation = null!;

    private sealed record WorldInfo(WorldData Data, WorldProvider Provider) : IWorldProvider.IWorldInfo
    {
        public string Name => Data.Information.Name;
        public string Version => Data.Information.Version;
        public DirectoryInfo Directory => Data.WorldDirectory;
        public DateTime DateTimeOfCreation => Data.Information.Creation;
        public DateTime? DateTimeOfLastLoad => Provider.GetDateTimeOfLastLoad(Data);
    }
}
