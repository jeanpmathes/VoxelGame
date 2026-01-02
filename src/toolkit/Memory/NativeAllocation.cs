// <copyright file="NativeAllocation.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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

namespace VoxelGame.Toolkit.Memory;

/// <summary>
///     Represents a memory allocation on a native heap.
///     This allocation must be freed by the allocator that created it.
/// </summary>
public readonly unsafe struct NativeAllocation<T> : IEquatable<NativeAllocation<T>> where T : unmanaged
{
    private readonly T* pointer;
    private readonly Int32 count;

    internal void* Pointer => pointer;

    internal NativeAllocation(T* pointer, Int32 count)
    {
        this.pointer = pointer;
        this.count = count;
    }

    /// <summary>
    ///     Get this allocation as a memory segment.
    /// </summary>
    public NativeSegment<T> Segment => new(pointer, count);

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(NativeAllocation<T> other)
    {
        return pointer == other.pointer && count == other.count;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is NativeAllocation<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(unchecked((Int32) (Int64) pointer), count);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(NativeAllocation<T> left, NativeAllocation<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(NativeAllocation<T> left, NativeAllocation<T> right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY
}
