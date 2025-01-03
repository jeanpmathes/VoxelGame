﻿// <copyright file="Casts.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Client.Logic.Chunks;
using VoxelGame.Client.Logic.Sections;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Provides utility extensions to simplify casts to client classes.
/// </summary>
public static class Casts
{
    /// <summary>
    ///     Cast a client world.
    /// </summary>
    public static World Cast(this Core.Logic.World chunk)
    {
        return (World) chunk;
    }

    /// <summary>
    ///     Cast a client chunk.
    /// </summary>
    public static Chunk Cast(this Core.Logic.Chunks.Chunk chunk)
    {
        return (Chunk) chunk;
    }

    /// <summary>
    ///     Cast a client section.
    /// </summary>
    public static Section Cast(this Core.Logic.Sections.Section section)
    {
        return (Section) section;
    }
}
