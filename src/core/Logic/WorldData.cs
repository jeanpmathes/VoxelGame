// <copyright file="WorldData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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

        Directory.CreateDirectory(WorldDirectory);
        Directory.CreateDirectory(ChunkDirectory);
        Directory.CreateDirectory(BlobDirectory);
        Directory.CreateDirectory(DebugDirectory);
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
