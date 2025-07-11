﻿// <copyright file="WorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.Providers;
using World = VoxelGame.Client.Logic.World;

namespace VoxelGame.Client.Application.Worlds;

/// <summary>
///     Provides worlds that are either loaded from disk or newly created.
///     The world provider itself does not active worlds.
/// </summary>
public partial class WorldProvider : IWorldProvider
{
    private readonly Client client;

    private readonly FileInfo metadataFile;

    private readonly List<IWorldProvider.IWorldInfo> worlds = [];
    private WorldDirectoryMetadata metadata = new();

    /// <summary>
    ///     Create a new world provider.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="worldsDirectory">The directory where worlds are loaded from and saved to.</param>
    internal WorldProvider(Client client, DirectoryInfo worldsDirectory)
    {
        this.client = client;

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
            Debug.Assert(Status == Status.Ok);

            return worlds;
        }
    }

    /// <inheritdoc />
    public Operation<Property> GetWorldProperties(IWorldProvider.IWorldInfo info)
    {
        return Operations.Launch(async token => await GetData(info).DeterminePropertiesAsync(token).InAnyContext());
    }

    /// <inheritdoc />
    public Operation Refresh()
    {
        Debug.Assert(Status != Status.Running);

        Status = Status.Running;

        worlds.Clear();

        Operation<Result<List<WorldData>>> refresh = Operations.Launch(async token =>
        {
            Result<WorldDirectoryMetadata> loaded = await WorldDirectoryMetadata.LoadAsync(metadataFile, token);

            metadata = loaded.UnwrapOrNull() ?? new WorldDirectoryMetadata();

            List<WorldData>? found;

            try
            {
                found = await SearchForWorldsAsync(token);

                LogWorldLookup(logger, found.Count);
            }
            catch (Exception searchException) when (searchException is IOException or SecurityException)
            {
                LogWorldRefreshError(logger, searchException);

                return Result.Error<List<WorldData>>(searchException);
            }

            List<String> obsoleteKeys = metadata.Entries.Keys.Except(found.Select(GetMetadataKey)).ToList();

            foreach (String key in obsoleteKeys)
                metadata.Entries.Remove(key);

            return Result.Ok(found);
        });

        refresh.OnCompletionSync(status =>
            {
                Status = status;
            },
            result =>
            {
                result.Switch(
                    found => worlds.AddRange(found.Select(data => new WorldInfo(data, this))),
                    _ => {});
            });

        return refresh;
    }

    /// <inheritdoc />
    public void LoadAndActivateWorld(IWorldProvider.IWorldInfo info)
    {
        Debug.Assert(Status == Status.Ok);

        World world = new(client, GetData(info));
        ActivateWorld(world);
    }

    /// <inheritdoc />
    public void CreateAndActivateWorld(String name)
    {
        (Int32 upper, Int32 lower) seed = (DateTime.UtcNow.GetHashCode(), RandomNumberGenerator.GetInt32(Int32.MinValue, Int32.MaxValue));

        DirectoryInfo worldDirectory = FileSystem.GetUniqueDirectory(WorldsDirectory, name);

        World world = new(client, worldDirectory, name, seed);
        ActivateWorld(world);
    }

    /// <inheritdoc />
    public Operation DeleteWorld(IWorldProvider.IWorldInfo info)
    {
        Debug.Assert(Status == Status.Ok);

        WorldData data = GetData(info);

        worlds.Remove(info);
        metadata.Entries.Remove(GetMetadataKey(data));

        return data.Delete();
    }

    /// <inheritdoc />
    public Operation DuplicateWorld(IWorldProvider.IWorldInfo info, String duplicateName)
    {
        Debug.Assert(Status == Status.Ok);

        WorldData data = GetData(info);

        DirectoryInfo newDirectory = FileSystem.GetUniqueDirectory(WorldsDirectory, duplicateName);

        Operation<WorldData> duplication = data.CopyTo(newDirectory).Then(async (duplicate, token) =>
        {
            await duplicate.RenameAsync(duplicateName, token).InAnyContext();

            return duplicate;
        });

        duplication.OnSuccessfulSync(result =>
        {
            worlds.Add(new WorldInfo(result, this));
        });

        return duplication;
    }

    /// <inheritdoc />
    public void RenameWorld(IWorldProvider.IWorldInfo info, String newName)
    {
        Debug.Assert(Status == Status.Ok);

        Operations.Launch(async token =>
        {
            await GetData(info).RenameAsync(newName, token).InAnyContext();
        });
    }

    /// <inheritdoc />
    public void SetFavorite(IWorldProvider.IWorldInfo info, Boolean isFavorite)
    {
        Debug.Assert(Status == Status.Ok);

        metadata.Entries.GetOrAdd(GetMetadataKey(GetData(info))).IsFavorite = isFavorite;

        SaveMetadata();
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public Boolean IsWorldNameValid(String name)
    {
        String valid = WorldData.MakeWorldNameValid(name);

        return valid == name;
    }

    private DateTime? GetDateTimeOfLastLoad(WorldData data)
    {
        Debug.Assert(Status == Status.Ok);

        metadata.Entries.TryGetValue(GetMetadataKey(data), out WorldFileMetadata? fileMetadata);

        return fileMetadata?.LastLoad;
    }

    private Boolean IsFavorite(WorldData data)
    {
        Debug.Assert(Status == Status.Ok);

        metadata.Entries.TryGetValue(GetMetadataKey(data), out WorldFileMetadata? fileMetadata);

        return fileMetadata?.IsFavorite ?? false;
    }

    private async Task<List<WorldData>> SearchForWorldsAsync(CancellationToken token = default)
    {
        List<WorldData> found = [];

        foreach (DirectoryInfo directory in WorldsDirectory.EnumerateDirectories())
        {
            token.ThrowIfCancellationRequested();

            if (WorldData.IsWorldDirectory(directory))
            {
                WorldData data = await WorldData.LoadInformationAsync(directory, token).InAnyContext();

                found.Add(data);

                LogValidWorldDirectory(logger, directory);
            }
            else
            {
                LogIgnoredDirectory(logger, directory);
            }
        }

        return found;
    }

    private void ActivateWorld(World world)
    {
        metadata.Entries.GetOrAdd(GetMetadataKey(world.Data)).LastLoad = DateTime.UtcNow;

        SaveMetadata();

        WorldActivation?.Invoke(this, world);
    }

    private static String GetMetadataKey(WorldData data)
    {
        return data.WorldDirectory.Name;
    }

    private static WorldData GetData(IWorldProvider.IWorldInfo info)
    {
        if (info is WorldInfo worldInfo)
            return worldInfo.Data;

        throw Exceptions.ArgumentOfWrongType(nameof(info), typeof(WorldInfo), info);
    }

    private void SaveMetadata()
    {
        Operations.Launch(async token =>
            {
                await metadata.SaveAsync(metadataFile, token).InAnyContext();
            },
            client.ClientUpdateDispatch);
    }

    /// <summary>
    ///     Is invoked when a world is requested to be activated.
    /// </summary>
    public event EventHandler<World>? WorldActivation;

    private sealed record WorldInfo(WorldData Data, WorldProvider Provider) : IWorldProvider.IWorldInfo
    {
        public String Name => Data.Information.Name;
        public String Version => Data.Information.Version;
        public DirectoryInfo Directory => Data.WorldDirectory;
        public DateTime DateTimeOfCreation => Data.Information.Creation;
        public DateTime? DateTimeOfLastLoad => Provider.GetDateTimeOfLastLoad(Data);
        public Boolean IsFavorite => Provider.IsFavorite(Data);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldProvider>();

    [LoggerMessage(EventId = LogID.WorldProvider + 0, Level = LogLevel.Information, Message = "Completed world lookup, found {Count} valid directories")]
    private static partial void LogWorldLookup(ILogger logger, Int32 count);

    [LoggerMessage(EventId = LogID.WorldProvider + 1, Level = LogLevel.Error, Message = "Failed to refresh worlds")]
    private static partial void LogWorldRefreshError(ILogger logger, Exception exception);

    [LoggerMessage(EventId = LogID.WorldProvider + 2, Level = LogLevel.Debug, Message = "Valid world directory found: {Directory}")]
    private static partial void LogValidWorldDirectory(ILogger logger, DirectoryInfo directory);

    [LoggerMessage(EventId = LogID.WorldProvider + 3, Level = LogLevel.Debug, Message = "Directory has no meta file and is ignored: {Directory}")]
    private static partial void LogIgnoredDirectory(ILogger logger, DirectoryInfo directory);

    #endregion
}
