// <copyright file="WorldData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
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
    public WorldData(DirectoryInfo directory)
    {
        WorldDirectory = directory;
        directory.Create();

        ChunkDirectory = FileSystem.CreateSubdirectory(directory, "Chunks");
        BlobDirectory = FileSystem.CreateSubdirectory(directory, "Blobs");
        DebugDirectory = FileSystem.CreateSubdirectory(directory, "Debug");
        ScriptDirectory = FileSystem.CreateSubdirectory(directory, "Scripts");
    }

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
    ///     Save the world information structure.
    /// </summary>
    public void SaveInformation(WorldInformation information)
    {
        information.Save(WorldDirectory.GetFile(MetaFileName));
    }

    /// <summary>
    ///     Load a world information structure.
    /// </summary>
    /// <param name="directory">The directory of the world.</param>
    /// <returns>The world information structure.</returns>
    public static WorldInformation LoadInformation(DirectoryInfo directory)
    {
        return WorldInformation.Load(directory.GetFile(MetaFileName));
    }

    /// <summary>
    ///     Check if a directory is a world directory.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>True if the directory is a world directory.</returns>
    public static bool IsWorldDirectory(DirectoryInfo directory)
    {
        return directory.GetFile(MetaFileName).Exists;
    }
}
