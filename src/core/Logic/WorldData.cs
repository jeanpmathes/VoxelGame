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

    private readonly List<DirectoryInfo> subdirectories = new();

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
    /// <param name="silent">If true, no log messages will be emitted.</param>
    public void EnsureValidInformation(bool silent = false)
    {
        uint validWorldSize = ClampSize(Information.Size);

        if (!silent && validWorldSize != Information.Size)
        {
            logger.LogWarning(Events.WorldState, "Loaded world size {Invalid} was invalid, changed to {Valid}", Information.Size, validWorldSize);
            Information.Size = validWorldSize;
        }

        Vector3d validSpawn = ClampSpawn(Information.SpawnInformation).Position;

        if (!silent && !VMath.NearlyEqual(validSpawn, Information.SpawnInformation.Position))
        {
            logger.LogWarning(Events.WorldState, "Loaded spawn position {Invalid} was invalid, changed to {Valid}", Information.SpawnInformation.Position, validSpawn);
            Information.SpawnInformation = new SpawnInformation(validSpawn);
        }
    }

    private static uint ClampSize(uint size)
    {
        return Math.Clamp(size, 16 * Chunk.BlockSize, World.BlockLimit - Chunk.BlockSize);
    }

    private SpawnInformation ClampSpawn(SpawnInformation spawn)
    {
        Vector3d size = new(Information.Size);
        Vector3d clamped = VMath.ClampComponents(spawn.Position, -size, size);

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
    ///     Get a reader for an existing blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <returns>The reader for the blob, or null if the blob does not exist.</returns>
    public BinaryReader? GetBlobReader(string name)
    {
        try
        {
            Stream stream = BlobDirectory.OpenFile(name, FileMode.Open, FileAccess.Read);

            return new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        }
        catch (IOException)
        {
            logger.LogDebug(Events.WorldIO, "Failed to read blob '{Name}'", name);

            return null;
        }
    }

    /// <summary>
    ///     Get a stream to a new blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <returns>The stream to the blob, or null if an error occurred.</returns>
    public BinaryWriter? GetBlobWriter(string name)
    {
        try
        {
            Stream stream = BlobDirectory.OpenFile(name, FileMode.Create, FileAccess.Write);

            return new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        }
        catch (IOException e)
        {
            logger.LogError(Events.WorldIO, e, "Failed to create blob '{Name}'", name);

            return null;
        }
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
        catch (IOException)
        {
            logger.LogDebug(Events.WorldIO, "Failed to read script '{Name}'", name);

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
}
