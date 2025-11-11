// <copyright file="NativeAllocator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Toolkit.Memory;

/// <summary>
///     Allocates memory on a native heap.
///     The allocator cannot be used concurrently from multiple threads.
///     If this object is disposed, all allocated memory will be freed.
///     The created allocations must be freed manually.
///     As they are structs, the GC will not even report that they are not disposed.
///     Using this class trades memory safety for performance, allowing memory allocation without GC overhead.
/// </summary>
public sealed class NativeAllocator : IDisposable
{
    private readonly IntPtr self;

    /// <summary>
    ///     Creates a new native allocator.
    /// </summary>
    public NativeAllocator()
    {
        self = NativeMethods.CreateAllocator();
    }

    /// <summary>
    ///     Allocates a block of typed memory on a native heap.
    /// </summary>
    /// <param name="count">The number of elements to allocate.</param>
    /// <typeparam name="T">The type of the elements to allocate.</typeparam>
    /// <returns>The allocated memory.</returns>
    public unsafe NativeAllocation<T> Allocate<T>(Int32 count) where T : unmanaged
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        Debug.Assert(count > 0);

        UInt64 size = (UInt64) count * (UInt64) sizeof(T);
        var pointer = (T*) NativeMethods.Allocate(self, size).ToPointer();

        if (pointer == null)
            throw Exceptions.InvalidOperation("Failed to allocate memory.");

        return new NativeAllocation<T>(pointer, count);
    }

    /// <summary>
    ///     Deallocates a block of memory allocated by this allocator.
    /// </summary>
    /// <param name="allocation">The allocation to deallocate.</param>
    /// <typeparam name="T">The type of the elements in the allocation.</typeparam>
    public unsafe void Deallocate<T>(NativeAllocation<T> allocation) where T : unmanaged
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        IntPtr pointer = new(allocation.Pointer);

        Int32 result = NativeMethods.Deallocate(self, pointer);

        Marshal.ThrowExceptionForHR(result);
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) NativeMethods.DeleteAllocator(self);
        else ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~NativeAllocator()
    {
        Dispose(disposing: false);
    }

    #endregion
}
