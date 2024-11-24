// <copyright file="NativeAllocation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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

    #region Equality Support

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

    #endregion Equality Support
}
