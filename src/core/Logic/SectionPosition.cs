// <copyright file="SectionPosition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The position of a section in the world.
/// </summary>
[Serializable]
public readonly struct SectionPosition : IEquatable<SectionPosition>
{
    /// <summary>
    ///     Create a section position with the given coordinates.
    /// </summary>
    public SectionPosition(int x, int y, int z)
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
    public bool Equals(SectionPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SectionPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static bool operator ==(SectionPosition left, SectionPosition right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static bool operator !=(SectionPosition left, SectionPosition right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Get this section position as a local position in a chunk.
    /// </summary>
    /// <returns>The local position in a chunk.</returns>
    public (int x, int y, int z) Local
    {
        get
        {
            int localX = X & (Logic.Chunk.Size - 1);
            int localY = Y & (Logic.Chunk.Size - 1);
            int localZ = Z & (Logic.Chunk.Size - 1);

            return (localX, localY, localZ);
        }
    }

    /// <summary>
    ///     Get the chunk this section position is in.
    /// </summary>
    /// <returns>The position of the chunk.</returns>
    public ChunkPosition Chunk
    {
        get
        {
            int chunkX = X >> Logic.Chunk.SizeExp;
            int chunkY = Y >> Logic.Chunk.SizeExp;
            int chunkZ = Z >> Logic.Chunk.SizeExp;

            return new ChunkPosition(chunkX, chunkY, chunkZ);
        }
    }

    /// <summary>
    ///     Offset this section position by the given amount.
    /// </summary>
    /// <param name="x">The x offset.</param>
    /// <param name="y">The y offset.</param>
    /// <param name="z">The z offset.</param>
    /// <returns>The offset section position.</returns>
    public SectionPosition Offset(int x, int y, int z)
    {
        return new SectionPosition(X + x, Y + y, Z + z);
    }

    /// <summary>
    ///     Check if this section contains the given world position.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <returns>True if the section contains the position.</returns>
    public bool Contains(Vector3i position)
    {
        return Equals(From(position));
    }

    /// <summary>
    ///     Get the offset that has to be applied to this section position to get the given position.
    /// </summary>
    /// <param name="other">The position to get the offset to.</param>
    /// <returns>The offset.</returns>
    public Vector3i OffsetTo(SectionPosition other)
    {
        return new Vector3i(other.X - X, other.Y - Y, other.Z - Z);
    }

    /// <summary>
    ///     Get the position of the first block in this section.
    /// </summary>
    public Vector3i FirstBlock => new Vector3i(X, Y, Z) * Section.Size;

    /// <summary>
    ///     Get the position of the last block in this section.
    /// </summary>
    public Vector3i LastBlock => FirstBlock + new Vector3i(Section.Size - 1);

    /// <summary>
    ///     Create a section position that contains a world position.
    /// </summary>
    public static SectionPosition From(Vector3i position)
    {
        int x = position.X >> Section.SizeExp;
        int y = position.Y >> Section.SizeExp;
        int z = position.Z >> Section.SizeExp;

        return new SectionPosition(x, y, z);
    }

    /// <summary>
    ///     Create a section position for a section in a given chunk, with the given local offsets.
    /// </summary>
    public static SectionPosition From(ChunkPosition position, (int x, int y, int z) localSection)
    {
        SectionPosition first = position.FirstSection;

        return new SectionPosition(first.X + localSection.x, first.Y + localSection.y, first.Z + localSection.z);
    }
}

