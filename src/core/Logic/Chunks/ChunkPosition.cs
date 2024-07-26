// <copyright file="ChunkPosition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     The position of a chunk in the world.
/// </summary>
public struct ChunkPosition : IEquatable<ChunkPosition>, IValue
{
    private Int32 x;
    private Int32 y;
    private Int32 z;

    /// <summary>
    ///     Create a chunk position with the given coordinates.
    /// </summary>
    public ChunkPosition(Int32 x, Int32 y, Int32 z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /// <summary>
    ///     The x coordinate.
    /// </summary>
    public Int32 X => x;

    /// <summary>
    ///     The y coordinate.
    /// </summary>
    public Int32 Y => y;

    /// <summary>
    ///     The z coordinate.
    /// </summary>
    public Int32 Z => z;

    /// <summary>
    ///     The equality comparison.
    /// </summary>
    public Boolean Equals(ChunkPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is ChunkPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(ChunkPosition left, ChunkPosition right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(ChunkPosition left, ChunkPosition right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Offset a chunk position by the given amount.
    /// </summary>
    public ChunkPosition Offset(Int32 xOffset, Int32 yOffset, Int32 zOffset)
    {
        return new ChunkPosition(X + xOffset, Y + yOffset, Z + zOffset);
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
    ///     Get the position of the chunk that contains the given world position.
    /// </summary>
    public static ChunkPosition From(Vector3i worldPosition)
    {
        (Int32 worldX, Int32 worldY, Int32 worldZ) = worldPosition;

        Int32 chunkX = worldX >> Chunk.BlockSizeExp;
        Int32 chunkY = worldY >> Chunk.BlockSizeExp;
        Int32 chunkZ = worldZ >> Chunk.BlockSizeExp;

        return new ChunkPosition(chunkX, chunkY, chunkZ);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return $"({X}|{Y}|{Z})";
    }

    /// <inheritdoc />
    public void Serialize(Serializer serializer)
    {
        serializer.Serialize(ref x);
        serializer.Serialize(ref y);
        serializer.Serialize(ref z);
    }
}
