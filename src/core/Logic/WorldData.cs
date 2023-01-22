// <copyright file="WorldData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Represents all data stored about a world on disk and provides utilities for IO operations.
/// </summary>
public class WorldData
{
    private const string MetaFileName = "meta.json";
    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldData>();

    /// <summary>
    ///     Creates a new world data object.
    /// </summary>
    /// <param name="directory">The directory of the world.</param>
    public WorldData(string directory)
    {
        WorldDirectory = directory;
        ChunkDirectory = Path.Combine(directory, "Chunks");
        BlobDirectory = Path.Combine(directory, "Blobs");
        DebugDirectory = Path.Combine(directory, "Debug");
        ScriptDirectory = Path.Combine(directory, "Scripts");

        Directory.CreateDirectory(WorldDirectory);
        Directory.CreateDirectory(ChunkDirectory);
        Directory.CreateDirectory(BlobDirectory);
        Directory.CreateDirectory(DebugDirectory);
        Directory.CreateDirectory(ScriptDirectory);
    }

    /// <summary>
    ///     The directory in which this world is stored.
    /// </summary>
    public string WorldDirectory { get; }

    /// <summary>
    ///     The directory in which all chunks of this world are stored.
    /// </summary>
    public string ChunkDirectory { get; }

    /// <summary>
    ///     The directory in named data blobs are stored.
    /// </summary>
    public string BlobDirectory { get; }

    /// <summary>
    ///     The directory at which debug artifacts can be stored.
    /// </summary>
    public string DebugDirectory { get; }

    /// <summary>
    ///     The directory in which scripts are stored.
    /// </summary>
    public string ScriptDirectory { get; }

    /// <summary>
    ///     Get a reader for an existing blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <returns>The reader for the blob, or null if the blob does not exist.</returns>
    public BinaryReader? GetBlobReader(string name)
    {
        try
        {
            Stream stream = File.Open(Path.Combine(BlobDirectory, name), FileMode.Open, FileAccess.Read);

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
            Stream stream = File.Open(Path.Combine(BlobDirectory, name), FileMode.Create, FileAccess.Write);

            return new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        }
        catch (IOException e)
        {
            logger.LogError(Events.WorldIO, e, "Failed to create blob '{Name}'", name);

            return null;
        }
    }

    private string GetScriptPath(string name)
    {
        return Path.Combine(ScriptDirectory, $"{name}.txt");
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
            return File.ReadAllText(GetScriptPath(name));
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
    public string? CreateScript(string name, string content)
    {
        try
        {
            string path = GetScriptPath(name);

            if (!File.Exists(path)) File.WriteAllText(path, content);

            return path;
        }
        catch (IOException e)
        {
            logger.LogError(Events.WorldIO, e, "Failed to create script '{Name}'", name);

            return null;
        }
    }

    /// <summary>
    ///     Save the world information structure.
    /// </summary>
    public void SaveInformation(WorldInformation information)
    {
        information.Save(Path.Combine(WorldDirectory, MetaFileName));
    }

    /// <summary>
    ///     Load a world information structure.
    /// </summary>
    /// <param name="directory">The directory of the world.</param>
    /// <returns>The world information structure.</returns>
    public static WorldInformation LoadInformation(string directory)
    {
        return WorldInformation.Load(Path.Combine(directory, MetaFileName));
    }

    /// <summary>
    ///     Check if a directory is a world directory.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>True if the directory is a world directory.</returns>
    public static bool IsWorldDirectory(string directory)
    {
        return File.Exists(Path.Combine(directory, MetaFileName));
    }
}

