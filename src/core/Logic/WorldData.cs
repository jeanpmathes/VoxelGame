// <copyright file="WorldData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Represents the data directory of a world and provides utilities to access and modify it.
/// </summary>
public partial class WorldData
{
    private const String InfoFileName = "info.json";

    private readonly List<DirectoryInfo> subdirectories = [];

    private readonly FileInfo informationFile;

    /// <summary>
    ///     Creates a new world data object.
    ///     This does not perform any IO operations.
    /// </summary>
    /// <param name="information">The information about the world.</param>
    /// <param name="directory">The directory of the world.</param>
    public WorldData(WorldInformation information, DirectoryInfo directory)
    {
        Information = information;
        WorldDirectory = directory;

        informationFile = directory.GetFile(InfoFileName);

        ChunkDirectory = AddSubdirectory("Chunks");
        BlobDirectory = AddSubdirectory("Blobs");
        DebugDirectory = AddSubdirectory("Debug");
        ScriptDirectory = AddSubdirectory("Scripts");

        DirectoryInfo AddSubdirectory(String name)
        {
            DirectoryInfo subdirectory = directory.GetDirectory(name);

            subdirectories.Add(subdirectory);

            return subdirectory;
        }
    }

    /// <summary>
    ///     Get the world information structure.
    /// </summary>
    public WorldInformation Information { get; }

    /// <summary>
    ///     The directory in which this world is stored.
    /// </summary>
    public DirectoryInfo WorldDirectory { get; }

    /// <summary>
    ///     The directory in which all chunks of this world are stored.
    /// </summary>
    public DirectoryInfo ChunkDirectory { get; }

    /// <summary>
    ///     The directory in named data blobs are stored.
    /// </summary>
    public DirectoryInfo BlobDirectory { get; }

    /// <summary>
    ///     The directory at which debug artifacts can be stored.
    /// </summary>
    public DirectoryInfo DebugDirectory { get; }

    /// <summary>
    ///     The directory in which scripts are stored.
    /// </summary>
    public DirectoryInfo ScriptDirectory { get; }

    /// <summary>
    ///     Ensure that the world information is valid.
    /// </summary>
    public void EnsureValidInformation()
    {
        MakeWorldInformationValid(Information, silent: true);
    }

    /// <summary>
    ///     Make the given world information valid.
    /// </summary>
    /// <param name="information">The information to make valid.</param>
    /// <param name="silent">If true, no log messages will be emitted.</param>
    private static void MakeWorldInformationValid(WorldInformation information, Boolean silent = false)
    {
        String validWorldName = MakeWorldNameValid(information.Name);

        if (!silent && validWorldName != information.Name)
        {
            LogInvalidWorldName(logger, information.Name, validWorldName);
            information.Name = validWorldName;
        }

        UInt32 validWorldSize = ClampSize(information.Size);

        if (!silent && validWorldSize != information.Size)
        {
            LogInvalidWorldSize(logger, information.Size, validWorldSize);
            information.Size = validWorldSize;
        }

        Vector3d validSpawn = ClampSpawn(information).Position;

        if (!silent && !MathTools.NearlyEqual(validSpawn, information.SpawnInformation.Position))
        {
            LogInvalidSpawnPosition(logger, information.SpawnInformation.Position, validSpawn);
            information.SpawnInformation = new SpawnInformation(validSpawn);
        }
    }

    /// <summary>
    ///     Create a valid world name from the given name.
    /// </summary>
    /// <param name="name">The name to make valid.</param>
    /// <returns>The valid name.</returns>
    public static String MakeWorldNameValid(String name)
    {
        StringBuilder builder = new(name.Length);

        foreach (Char c in name.Trim())
        {
            if (Array.Exists(Path.GetInvalidFileNameChars(), value => value == c)) continue;
            if (Array.Exists(['.', ',', '{', '}'], value => value == c)) continue;

            builder.Append(c);

            if (builder.Length == 64) break;
        }

        if (builder.Length == 0)
            builder.Append("World");

        return builder.ToString();
    }

    private static UInt32 ClampSize(UInt32 size)
    {
        return Math.Clamp(size, 16 * Chunk.BlockSize, World.BlockLimit - Chunk.BlockSize);
    }

    private static SpawnInformation ClampSpawn(WorldInformation information)
    {
        Vector3d size = new(information.Size);
        Vector3d clamped = MathTools.ClampComponents(information.SpawnInformation.Position, -size, size);

        return new SpawnInformation(clamped);
    }

    /// <summary>
    ///     Ensure that all directories exist and are valid.
    /// </summary>
    public void EnsureValidDirectory()
    {
        // While it is technically possible that the following methods throw exceptions,
        // we can reasonably assume that we have write access to the world directory.

        WorldDirectory.Create();

        foreach (DirectoryInfo subdirectory in subdirectories)
            subdirectory.Create();
    }

    /// <summary>
    ///     Read in a data blob that contains a serialized entity.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>The entity, or null if an error occurred.</returns>
    public T? ReadBlob<T>(String name) where T : class, IEntity, new()
    {
        Result<T> result = Serialize.LoadBinary<T>(BlobDirectory.GetFile(name), typeof(T).FullName ?? "");

        return result.Switch(
            T? (blob) => blob,
            exception =>
            {
                if (exception is FileFormatException) LogFailedToReadBlob(logger, exception, name);
                else LogFailedToReadBlobDebug(logger, exception, name);

                return null;
            });
    }

    /// <summary>
    ///     Write an entity to a data blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <param name="entity">The entity to write.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public void WriteBlob<T>(String name, T entity) where T : class, IEntity, new()
    {
        Result result = Serialize.SaveBinary(entity, BlobDirectory.GetFile(name), typeof(T).FullName ?? "");

        result.Switch(
            () => {},
            exception => LogFailedToWriteBlob(logger, exception, name));
    }

    private FileInfo GetScriptPath(String name)
    {
        return ScriptDirectory.GetFile($"{name}.txt");
    }

    /// <summary>
    ///     Get the content of a script.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <returns>The content of the script, or null if the script does not exist.</returns>
    public String? GetScript(String name)
    {
        try
        {
            return GetScriptPath(name).ReadAllText();
        }
        catch (IOException e)
        {
            LogFailedToReadScript(logger, e, name);

            return null;
        }
    }

    /// <summary>
    ///     Create a new script. If the script already exists, it will not be changed but the path will still be returned.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <param name="content">The initial content of the script.</param>
    /// <returns>The path to the script, or null if an error occurred.</returns>
    public FileInfo? CreateScript(String name, String content)
    {
        try
        {
            FileInfo script = GetScriptPath(name);

            if (!script.Exists) script.WriteAllText(content);

            return script;
        }
        catch (IOException e)
        {
            LogFailedToCreateScript(logger, e, name);

            return null;
        }
    }

    /// <summary>
    ///     Save all information directly handled by this class.
    ///     This will not save any chunks or open blobs.
    /// </summary>
    public async Task SaveAsync(CancellationToken token = default)
    {
        await Information.SaveAsync(informationFile, token).InAnyContext();
    }

    /// <summary>
    ///     Load a world information structure and create the world data class.
    /// </summary>
    /// <param name="directory">The directory of the world.</param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>The world information structure.</returns>
    public static async Task<WorldData> LoadInformationAsync(DirectoryInfo directory, CancellationToken token = default)
    {
        WorldInformation information = await WorldInformation.LoadAsync(directory.GetFile(InfoFileName), token).InAnyContext();

        MakeWorldInformationValid(information);

        return new WorldData(information, directory);
    }

    /// <summary>
    ///     Check if a directory is a world directory.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>True if the directory is a world directory.</returns>
    public static Boolean IsWorldDirectory(DirectoryInfo directory)
    {
        return directory.GetFile(InfoFileName).Exists;
    }

    /// <summary>
    ///     Determine the properties of the world.
    /// </summary>
    /// <param name="token">A token to cancel the operation.</param>
    public async Task<Property> DeterminePropertiesAsync(CancellationToken token = default)
    {
        return await WorldProperties.CreateAsync(Information, WorldDirectory, token).InAnyContext();
    }

    /// <summary>
    ///     Delete the world and all its data.
    ///     This starts an operation on a background thread.
    /// </summary>
    public Operation Delete()
    {
        try
        {
            // By deleting the information file first and on the main thread, we invalidate the world directory.
            // As such, any searches for worlds will ignore this directory, even if the deletion is still in progress.

            informationFile.Delete();
        }
        catch (Exception e) when (e is IOException or SecurityException or UnauthorizedAccessException)
        {
            // Ignore, because the next step will it try again and log the error.
        }

        return Operations.Launch(_ =>
        {
            try
            {
                WorldDirectory.Delete(recursive: true);

                LogDeletedWorld(logger, Information.Name);
            }
            catch (Exception e) when (e is IOException or SecurityException or UnauthorizedAccessException)
            {
                LogFailedToDeleteWorld(logger, e);

                throw;
            }

            return Task.CompletedTask;
        });
    }

    /// <summary>
    ///     Copy this world to another directory.
    /// </summary>
    /// <param name="targetDirectory">The directory to copy the world to.</param>
    /// <returns>The operation that will copy the world.</returns>
    public Operation<WorldData> CopyTo(DirectoryInfo targetDirectory)
    {
        return Operations.Launch(async token =>
        {
            try
            {
                await WorldDirectory.CopyToAsync(targetDirectory, token).InAnyContext();

                LogCopiedWorld(logger, Information.Name, targetDirectory.FullName);

                return await LoadInformationAsync(targetDirectory, token).InAnyContext();
            }
            catch (Exception e) when (e is IOException or SecurityException or UnauthorizedAccessException)
            {
                LogFailedToCopyWorld(logger, e);

                throw;
            }
        });
    }

    /// <summary>
    ///     Rename the world.
    /// </summary>
    /// <param name="newName">The new name of the world. Must be a valid name.</param>
    /// <param name="token">A token to cancel the operation.</param>
    public async Task RenameAsync(String newName, CancellationToken token = default)
    {
        LogRenamingWorld(logger, Information.Name, newName);

        Information.Name = newName;

        await SaveAsync(token).InAnyContext();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldData>();

    [LoggerMessage(EventId = LogID.WorldData + 0, Level = LogLevel.Warning, Message = "Loaded world name '{Invalid}' was invalid, changed to '{Valid}'")]
    private static partial void LogInvalidWorldName(ILogger logger, String invalid, String valid);

    [LoggerMessage(EventId = LogID.WorldData + 1, Level = LogLevel.Warning, Message = "Loaded world size {Invalid} was invalid, changed to {Valid}")]
    private static partial void LogInvalidWorldSize(ILogger logger, UInt32 invalid, UInt32 valid);

    [LoggerMessage(EventId = LogID.WorldData + 2, Level = LogLevel.Warning, Message = "Loaded spawn position {Invalid} was invalid, changed to {Valid}")]
    private static partial void LogInvalidSpawnPosition(ILogger logger, Vector3d invalid, Vector3d valid);

    [LoggerMessage(EventId = LogID.WorldData + 3, Level = LogLevel.Error, Message = "Failed to read blob '{Name}', format is incorrect")]
    private static partial void LogFailedToReadBlob(ILogger logger, Exception exception, String name);

    [LoggerMessage(EventId = LogID.WorldData + 4, Level = LogLevel.Debug, Message = "Failed to read blob '{Name}'")]
    private static partial void LogFailedToReadBlobDebug(ILogger logger, Exception exception, String name);

    [LoggerMessage(EventId = LogID.WorldData + 5, Level = LogLevel.Error, Message = "Failed to write blob '{Name}'")]
    private static partial void LogFailedToWriteBlob(ILogger logger, Exception exception, String name);

    [LoggerMessage(EventId = LogID.WorldData + 6, Level = LogLevel.Debug, Message = "Failed to read script '{Name}'")]
    private static partial void LogFailedToReadScript(ILogger logger, Exception exception, String name);

    [LoggerMessage(EventId = LogID.WorldData + 7, Level = LogLevel.Error, Message = "Failed to create script '{Name}'")]
    private static partial void LogFailedToCreateScript(ILogger logger, Exception exception, String name);

    [LoggerMessage(EventId = LogID.WorldData + 8, Level = LogLevel.Information, Message = "Deleted world '{Name}'")]
    private static partial void LogDeletedWorld(ILogger logger, String name);

    [LoggerMessage(EventId = LogID.WorldData + 9, Level = LogLevel.Error, Message = "Failed to delete world")]
    private static partial void LogFailedToDeleteWorld(ILogger logger, Exception exception);

    [LoggerMessage(EventId = LogID.WorldData + 10, Level = LogLevel.Information, Message = "Copied world '{Name}' to '{Target}'")]
    private static partial void LogCopiedWorld(ILogger logger, String name, String target);

    [LoggerMessage(EventId = LogID.WorldData + 11, Level = LogLevel.Error, Message = "Failed to copy world")]
    private static partial void LogFailedToCopyWorld(ILogger logger, Exception exception);

    [LoggerMessage(EventId = LogID.WorldData + 12, Level = LogLevel.Information, Message = "Renaming world '{OldName}' to '{NewName}'")]
    private static partial void LogRenamingWorld(ILogger logger, String oldName, String newName);

    #endregion LOGGING
}
