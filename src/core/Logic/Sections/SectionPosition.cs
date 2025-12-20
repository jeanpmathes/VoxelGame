// <copyright file="SectionPosition.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;

namespace VoxelGame.Core.Logic.Sections;

/// <summary>
///     The position of a section in the world.
/// </summary>
public readonly struct SectionPosition : IEquatable<SectionPosition>
{
    /// <summary>
    ///     Create a section position with the given coordinates.
    /// </summary>
    public SectionPosition(Int32 x, Int32 y, Int32 z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    ///     The x coordinate.
    /// </summary>
    public Int32 X { get; init; }

    /// <summary>
    ///     The y coordinate.
    /// </summary>
    public Int32 Y { get; init; }

    /// <summary>
    ///     The z coordinate.
    /// </summary>
    public Int32 Z { get; init; }

    /// <summary>
    ///     The equality comparison.
    /// </summary>
    public Boolean Equals(SectionPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is SectionPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(SectionPosition left, SectionPosition right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(SectionPosition left, SectionPosition right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Get this section position as a local position in a chunk.
    /// </summary>
    /// <returns>The local position in a chunk.</returns>
    public (Int32 x, Int32 y, Int32 z) Local
    {
        get
        {
            Int32 localX = X & (Chunks.Chunk.Size - 1);
            Int32 localY = Y & (Chunks.Chunk.Size - 1);
            Int32 localZ = Z & (Chunks.Chunk.Size - 1);

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
            Int32 chunkX = X >> Chunks.Chunk.SizeExp;
            Int32 chunkY = Y >> Chunks.Chunk.SizeExp;
            Int32 chunkZ = Z >> Chunks.Chunk.SizeExp;

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
    public SectionPosition Offset(Int32 x, Int32 y, Int32 z)
    {
        return new SectionPosition(X + x, Y + y, Z + z);
    }

    /// <summary>
    ///     Check if this section contains the given world position.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <returns>True if the section contains the position.</returns>
    public Boolean Contains(Vector3i position)
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

    /// <inheritdoc />
    public override String ToString()
    {
        return $"S({X}, {Y}, {Z})";
    }

    /// <summary>
    ///     Create a section position that contains a world position.
    /// </summary>
    public static SectionPosition From(Vector3i position)
    {
        Int32 x = position.X >> Section.SizeExp;
        Int32 y = position.Y >> Section.SizeExp;
        Int32 z = position.Z >> Section.SizeExp;

        return new SectionPosition(x, y, z);
    }

    /// <summary>
    ///     Create a section position for a section in a given chunk, with the given local offsets.
    /// </summary>
    public static SectionPosition From(ChunkPosition position, (Int32 x, Int32 y, Int32 z) localSection)
    {
        SectionPosition first = position.FirstSection;

        return new SectionPosition(first.X + localSection.x, first.Y + localSection.y, first.Z + localSection.z);
    }
}
