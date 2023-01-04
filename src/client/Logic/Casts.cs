// <copyright file="Casts.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Provides utility extensions to simplify casts to client classes.
/// </summary>
public static class Casts
{
    /// <summary>
    ///     Cast a client world.
    /// </summary>
    public static ClientWorld Cast(this World chunk)
    {
        return (ClientWorld) chunk;
    }

    /// <summary>
    ///     Cast a client chunk.
    /// </summary>
    public static ClientChunk Cast(this Chunk chunk)
    {
        return (ClientChunk) chunk;
    }

    /// <summary>
    ///     Cast a client section.
    /// </summary>
    public static ClientSection Cast(this Section section)
    {
        return (ClientSection) section;
    }
}

