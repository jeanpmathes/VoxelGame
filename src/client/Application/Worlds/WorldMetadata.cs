// <copyright file="WorldMetadata.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Client.Application.Worlds;

#pragma warning disable S4004 // Unused getters required for JSON serialization.

/// <summary>
///     Metadata associated with a world, but not stored as part of the world.
///     The data is in relation to the world in the context of this client.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class WorldFileMetadata
{
    /// <summary>
    ///     The time of the last load of the world, or null if the world has never been loaded.
    /// </summary>
    public DateTime? LastLoad { get; set; }

    /// <summary>
    ///     Whether the world is marked as a favorite.
    /// </summary>
    public Boolean IsFavorite { get; set; }
}

/// <summary>
///     Metadata for all worlds in the worlds directory.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public partial class WorldDirectoryMetadata
{
    /// <summary>
    ///     A dictionary from world directory name to the metadata of the world.
    /// </summary>
    public Dictionary<String, WorldFileMetadata> Entries { get; set; } = new();

    /// <summary>
    ///     Save the metadata to a file.
    /// </summary>
    /// <param name="file">The file to save to.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task SaveAsync(FileInfo file, CancellationToken token = default)
    {
        Result result = await Serialize.SaveJsonAsync(this, file, token).InAnyContext();

        result.Switch(
            () => LogSaveMetadataSuccess(logger, file),
            exception => LogSaveMetadataFailure(logger, exception, file));
    }

    /// <summary>
    ///     Load the metadata from a file.
    ///     If the file does not exist, empty metadata is returned and this is not considered a failure.
    ///     Other errors are considered failures.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The metadata result.</returns>
    public static async Task<Result<WorldDirectoryMetadata>> LoadAsync(FileInfo file, CancellationToken token = default)
    {
        if (!file.Exists)
        {
            LogMetadataFileDoesNotExist(logger, file);

            return Result.Ok(new WorldDirectoryMetadata());
        }

        Result<WorldDirectoryMetadata> result = await Serialize.LoadJsonAsync<WorldDirectoryMetadata>(file, token).InAnyContext();

        result.Switch(
            _ => LogLoadMetadataSuccess(logger, file),
            exception => LogLoadMetadataFailure(logger, exception, file));

        return result;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldDirectoryMetadata>();

    [LoggerMessage(EventId = LogID.WorldMetadata + 0, Level = LogLevel.Debug, Message = "World directory metadata saved to {File}")]
    private static partial void LogSaveMetadataSuccess(ILogger logger, FileInfo file);

    [LoggerMessage(EventId = LogID.WorldMetadata + 1, Level = LogLevel.Error, Message = "Failed to save world directory metadata to {File}")]
    private static partial void LogSaveMetadataFailure(ILogger logger, Exception exception, FileInfo file);

    [LoggerMessage(EventId = LogID.WorldMetadata + 2, Level = LogLevel.Debug, Message = "World directory metadata file does not exist: {File}")]
    private static partial void LogMetadataFileDoesNotExist(ILogger logger, FileInfo file);

    [LoggerMessage(EventId = LogID.WorldMetadata + 3, Level = LogLevel.Debug, Message = "World directory metadata loaded from {File}")]
    private static partial void LogLoadMetadataSuccess(ILogger logger, FileInfo file);

    [LoggerMessage(EventId = LogID.WorldMetadata + 4, Level = LogLevel.Error, Message = "Failed to load world directory metadata from {File}")]
    private static partial void LogLoadMetadataFailure(ILogger logger, Exception exception, FileInfo file);

    #endregion LOGGING
}
