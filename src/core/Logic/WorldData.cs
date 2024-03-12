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
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Represents the data directory of a world and provides utilities to access and modify it.
/// </summary>
public class WorldData
{
    private const string InfoFileName = "info.json";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldData>();

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

        DirectoryInfo AddSubdirectory(string name)
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
    private static void MakeWorldInformationValid(WorldInformation information, bool silent = false)
    {
        string validWorldName = MakeWorldNameValid(information.Name);

        if (!silent && validWorldName != information.Name)
        {
            logger.LogWarning(Events.WorldState, "Loaded world name '{Invalid}' was invalid, changed to '{Valid}'", information.Name, validWorldName);
            information.Name = validWorldName;
        }

        uint validWorldSize = ClampSize(information.Size);

        if (!silent && validWorldSize != information.Size)
        {
            logger.LogWarning(Events.WorldState, "Loaded world size {Invalid} was invalid, changed to {Valid}", information.Size, validWorldSize);
            information.Size = validWorldSize;
        }

        Vector3d validSpawn = ClampSpawn(information).Position;

        if (!silent && !VMath.NearlyEqual(validSpawn, information.SpawnInformation.Position))
        {
            logger.LogWarning(Events.WorldState, "Loaded spawn position {Invalid} was invalid, changed to {Valid}", information.SpawnInformation.Position, validSpawn);
            information.SpawnInformation = new SpawnInformation(validSpawn);
        }
    }

    /// <summary>
    ///     Create a valid world name from the given name.
    /// </summary>
    /// <param name="name">The name to make valid.</param>
    /// <returns>The valid name.</returns>
    public static string MakeWorldNameValid(string name)
    {
        StringBuilder builder = new(name.Length);

        foreach (char c in name.Trim())
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

    private static uint ClampSize(uint size)
    {
        return Math.Clamp(size, 16 * Chunk.BlockSize, World.BlockLimit - Chunk.BlockSize);
    }

    private static SpawnInformation ClampSpawn(WorldInformation information)
    {
        Vector3d size = new(information.Size);
        Vector3d clamped = VMath.ClampComponents(information.SpawnInformation.Position, -size, size);

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
    /// Read in a data blob that contains a serialized entity.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>The entity, or null if an error occurred.</returns>
    public T? ReadBlob<T>(string name) where T : class, IEntity, new()
    {
        Exception? exception = Serialize.LoadBinary(BlobDirectory.GetFile(name), out T entity, typeof(T).FullName ?? "");

        if (exception is FileFormatException) logger.LogError(Events.WorldIO, exception, "Failed to read blob '{Name}', format is incorrect", name);
        else if (exception != null) logger.LogDebug(Events.WorldIO, exception, "Failed to read blob '{Name}'", name);
        else return entity;

        return null;
    }

    /// <summary>
    /// Write an entity to a data blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <param name="entity">The entity to write.</param>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public void WriteBlob<T>(string name, T entity) where T : class, IEntity, new()
    {
        Exception? exception = Serialize.SaveBinary(entity, BlobDirectory.GetFile(name), typeof(T).FullName ?? "");

        if (exception != null)
            logger.LogError(Events.WorldIO, exception, "Failed to write blob '{Name}'", name);
    }

    private FileInfo GetScriptPath(string name)
    {
        return ScriptDirectory.GetFile($"{name}.txt");
    }

    /// <summary>
    ///     Get the content of a script.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <returns>The content of the script, or null if the script does not exist.</returns>
    public string? GetScript(string name)
    {
        try
        {
            return GetScriptPath(name).ReadAllText();
        }
        catch (IOException e)
        {
            logger.LogDebug(Events.WorldIO, e, "Failed to read script '{Name}'", name);

            return null;
        }
    }

    /// <summary>
    ///     Create a new script. If the script already exists, it will not be changed but the path will still be returned.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <param name="content">The initial content of the script.</param>
    /// <returns>The path to the script, or null if an error occurred.</returns>
    public FileInfo? CreateScript(string name, string content)
    {
        try
        {
            FileInfo script = GetScriptPath(name);

            if (!script.Exists) script.WriteAllText(content);

            return script;
        }
        catch (IOException e)
        {
            logger.LogError(Events.WorldIO, e, "Failed to create script '{Name}'", name);

            return null;
        }
    }

    /// <summary>
    /// Save all information directly handled by this class.
    /// This will not save any chunks or open blobs.
    /// </summary>
    public void Save()
    {
        Information.Save(informationFile);
    }

    /// <summary>
    ///     Load a world information structure and create the world data class.
    /// </summary>
    /// <param name="directory">The directory of the world.</param>
    /// <returns>The world information structure.</returns>
    public static WorldData LoadInformation(DirectoryInfo directory)
    {
        WorldInformation information = WorldInformation.Load(directory.GetFile(InfoFileName));

        MakeWorldInformationValid(information);

        return new WorldData(information, directory);
    }

    /// <summary>
    ///     Check if a directory is a world directory.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>True if the directory is a world directory.</returns>
    public static bool IsWorldDirectory(DirectoryInfo directory)
    {
        return directory.GetFile(InfoFileName).Exists;
    }

    /// <summary>
    ///     Determine the properties of the world.
    /// </summary>
    public Property DetermineProperties()
    {
        return new WorldProperties(Information, WorldDirectory);
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

        return Operations.Launch(() =>
        {
            try
            {
                WorldDirectory.Delete(recursive: true);

                logger.LogInformation(Events.WorldIO, "Deleted world '{Name}'", Information.Name);
            }
            catch (Exception e) when (e is IOException or SecurityException or UnauthorizedAccessException)
            {
                logger.LogError(Events.WorldIO, e, "Failed to delete world");

                throw;
            }
        });
    }

    /// <summary>
    ///     Copy this world to another directory.
    /// </summary>
    /// <param name="targetDirectory">The directory to copy the world to.</param>
    /// <returns>The operation that will copy the world.</returns>
    public Operation<WorldData> CopyTo(DirectoryInfo targetDirectory)
    {
        return Operations.Launch(() =>
        {
            try
            {
                WorldDirectory.CopyTo(targetDirectory);

                logger.LogInformation(Events.WorldIO, "Copied world '{Name}' to '{Target}'", Information.Name, targetDirectory.FullName);

                return LoadInformation(targetDirectory);
            }
            catch (Exception e) when (e is IOException or SecurityException or UnauthorizedAccessException)
            {
                logger.LogError(Events.WorldIO, e, "Failed to copy world");

                throw;
            }
        });
    }

    /// <summary>
    ///     Rename the world.
    /// </summary>
    /// <param name="newName">The new name of the world. Must be a valid name.</param>
    public void Rename(string newName)
    {
        logger.LogInformation(Events.WorldIO, "Renaming world '{OldName}' to '{NewName}'", Information.Name, newName);

        Information.Name = newName;

        Save();
    }
}
