// <copyright file="Request.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     A request for a chunk (and the surrounding chunks) to be loaded.
/// </summary>
/// <param name="position">The position of the requested chunk.</param>
/// <param name="requester">The actor requesting the chunk.</param>
public class Request(ChunkPosition position, Actor requester)
{
    /// <summary>
    ///     Get the position of the requested chunk.
    /// </summary>
    public ChunkPosition Position { get; } = position;

    /// <summary>
    ///     Get the actor requesting the chunk.
    /// </summary>
    public Actor Requester { get; } = requester;

    /// <summary>
    ///     Internal index used by <see cref="Requests" />.
    /// </summary>
    internal Int32? Index { get; set; }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Position, Requester);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is not Request request)
            return false;

        return Position == request.Position && Requester == request.Requester;
    }
}
