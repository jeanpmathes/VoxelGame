// <copyright file="ChunkPosition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The position of a chunk in the world.
/// </summary>
[Serializable]
public readonly struct ChunkPosition : IEquatable<ChunkPosition>
{
    /// <summary>
    ///     Create a chunk position with the given coordinates.
    /// </summary>
    public ChunkPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    ///     The x coordinate.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    ///     The y coordinate.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    ///     The z coordinate.
    /// </summary>
    public int Z { get; init; }

    /// <summary>
    ///     The equality comparison.
    /// </summary>
    public bool Equals(ChunkPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ChunkPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static bool operator ==(ChunkPosition left, ChunkPosition right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static bool operator !=(ChunkPosition left, ChunkPosition right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Offset a chunk position by the given amount.
    /// </summary>
    public ChunkPosition Offset(int x, int y, int z)
    {
        return new ChunkPosition(X + x, Y + y, Z + z);
    }

    /// <summary>
    ///     Offset a chunk position by the given amount.
    /// </summary>
    public ChunkPosition Offset(Vector3i amount)
    {
        return Offset(amount.X, amount.Y, amount.Z);
    }

    /// <summary>
    ///     Get the offset that has to be applied to this chunk position to get the given position.
    /// </summary>
    /// <param name="other">The position to get the offset to.</param>
    /// <returns>The offset.</returns>
    public Vector3i OffsetTo(ChunkPosition other)
    {
        return new Vector3i(other.X - X, other.Y - Y, other.Z - Z);
    }

    /// <summary>
    ///     Get the center of this chunk position.
    /// </summary>
    public Vector3d Center => new(
        X * Chunk.BlockSize + Chunk.BlockSize / 2f,
        Y * Chunk.BlockSize + Chunk.BlockSize / 2f,
        Z * Chunk.BlockSize + Chunk.BlockSize / 2f);

    /// <summary>
    ///     Get the position of the first section of the chunk at this position.
    /// </summary>
    public SectionPosition FirstSection => new(X * Chunk.Size, Y * Chunk.Size, Z * Chunk.Size);

    /// <summary>
    ///     Get the origin position, which is (0, 0, 0).
    /// </summary>
    public static ChunkPosition Origin => new(x: 0, y: 0, z: 0);

    /// <summary>
    ///     Get the position of the chunk that contains the given world position.
    /// </summary>
    public static ChunkPosition From(Vector3i worldPosition)
    {
        (int x, int y, int z) = worldPosition;

        int chunkX = x >> Chunk.BlockSizeExp;
        int chunkY = y >> Chunk.BlockSizeExp;
        int chunkZ = z >> Chunk.BlockSizeExp;

        return new ChunkPosition(chunkX, chunkY, chunkZ);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"({X}|{Y}|{Z})";
    }
}

