// <copyright file="Streams.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.IO;

namespace VoxelGame.Toolkit.Memory;

/// <summary>
///     Utility to help with streams.
/// </summary>
public class Streams
{
    private readonly RecyclableMemoryStreamManager manager = new();

    /// <summary>
    ///     Get the shared instance of the streams utility.
    /// </summary>
    public static Streams Shared { get; } = new();

    /// <summary>
    ///     Get a pooled memory stream.
    /// </summary>
    /// <returns>The pooled memory stream. Must be disposed.</returns>
    public MemoryStream GetPooledMemoryStream()
    {
        return manager.GetStream();
    }
}
