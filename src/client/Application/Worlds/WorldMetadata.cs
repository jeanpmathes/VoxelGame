// <copyright file="WorldMetadata.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Serialization;
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
    public void Save(FileInfo file)
    {
        Exception? exception = Serialize.SaveJSON(this, file);

        if (exception == null)
            LogSaveMetadataSuccess(logger, file);
        else
            LogSaveMetadataFailure(logger, exception, file);
    }

    /// <summary>
    ///     Load the metadata from a file.
    ///     If the file does not exist, empty metadata is returned and this is not considered a failure.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="exception">The exception that occurred during loading, if any. </param>
    /// <returns>The metadata.</returns>
    public static WorldDirectoryMetadata Load(FileInfo file, out Exception? exception)
    {
        exception = Serialize.LoadJSON(file, out WorldDirectoryMetadata metadata);

        if (!file.Exists)
        {
            LogMetadataFileDoesNotExist(logger, file);
            exception = null;
        }
        else if (exception == null)
        {
            LogLoadMetadataSuccess(logger, file);
        }
        else
        {
            LogLoadMetadataFailure(logger, exception, file);
        }

        return metadata;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldDirectoryMetadata>();

    [LoggerMessage(EventId = Events.FileIO, Level = LogLevel.Debug, Message = "World directory metadata saved to {File}")]
    private static partial void LogSaveMetadataSuccess(ILogger logger, FileInfo file);

    [LoggerMessage(EventId = Events.FileIO, Level = LogLevel.Error, Message = "Failed to save world directory metadata to {File}")]
    private static partial void LogSaveMetadataFailure(ILogger logger, Exception exception, FileInfo file);

    [LoggerMessage(EventId = Events.FileIO, Level = LogLevel.Debug, Message = "World directory metadata file does not exist: {File}")]
    private static partial void LogMetadataFileDoesNotExist(ILogger logger, FileInfo file);

    [LoggerMessage(EventId = Events.FileIO, Level = LogLevel.Debug, Message = "World directory metadata loaded from {File}")]
    private static partial void LogLoadMetadataSuccess(ILogger logger, FileInfo file);

    [LoggerMessage(EventId = Events.FileIO, Level = LogLevel.Error, Message = "Failed to load world directory metadata from {File}")]
    private static partial void LogLoadMetadataFailure(ILogger logger, Exception exception, FileInfo file);

    #endregion LOGGING
}
