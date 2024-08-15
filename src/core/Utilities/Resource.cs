// <copyright file="Resource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Different access states to a resource.
/// </summary>
public enum Access
{
    /// <summary>
    ///     Read access.
    /// </summary>
    Read,

    /// <summary>
    ///     Write access, implies read access.
    /// </summary>
    Write,

    /// <summary>
    ///     No access.
    /// </summary>
    None
}

/// <summary>
///     Represents an abstract resource that can be acquired and released.
///     The resource allows multiple readers or one writer at a time.
/// </summary>
public sealed class Resource
{
    private readonly String name;
    private Boolean isWrittenTo;
    private Int32 readerCount;

    private WeakReference<Guard>? writer; // todo: remove
    private readonly List<WeakReference<Guard>> readers = []; // todo: remove

    /// <summary>
    ///     Creates a new resource with the given name.
    /// </summary>
    /// <param name="name"></param>
    public Resource(String name)
    {
        this.name = name;
    }

    /// <summary>
    ///     Get whether there is any reader or writer currently using the resource.
    /// </summary>
    public Boolean IsAcquired => readerCount > 0 || isWrittenTo;

    /// <summary>
    ///     Get whether it is possible to read from the resource.
    /// </summary>
    public Boolean CanRead => !isWrittenTo;

    /// <summary>
    ///     Get whether it is possible to write to the resource.
    /// </summary>
    public Boolean CanWrite => readerCount == 0 && CanRead;

    /// <summary>
    ///     Check if this resource is currently held by a specific guard.
    /// </summary>
    /// <param name="guard">The guard to check.</param>
    /// <param name="access">The access level to check for.</param>
    /// <returns>True if the guard holds the resource with the specified access level.</returns>
    public Boolean IsHeldBy(Guard guard, Access access)
    {
        return access switch
        {
            Access.Read => guard.IsGuarding(this) && !isWrittenTo,
            Access.Write => guard.IsGuarding(this) && isWrittenTo,
            _ => false
        };
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return name;
    }

    private void ReleaseReader()
    {
        if (readerCount == 0) Debug.Fail("No reader to release.");
        else readerCount--;

        if (readerCount == 0) Released?.Invoke(this, EventArgs.Empty);

        readers.RemoveAll(wr => !wr.TryGetTarget(out _));
    }

    private void ReleaseWriter()
    {
        if (!isWrittenTo) Debug.Fail("No writer to release.");
        else isWrittenTo = false;

        Released?.Invoke(this, EventArgs.Empty);

        writer = null;
    }

    /// <summary>
    ///     Try to acquire the resource for reading.
    /// </summary>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    private Guard? TryAcquireReader()
    {
        Throw.IfNotOnMainThread(this);

        if (!CanRead) return null;

        readerCount++;

        Guard guard = new(this, ReleaseReader);
        readers.Add(new WeakReference<Guard>(guard));

        return guard;
    }

    /// <summary>
    ///     Try to acquire the resource for writing.
    /// </summary>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    private Guard? TryAcquireWriter()
    {
        Throw.IfNotOnMainThread(this);

        if (!CanWrite) return null;

        isWrittenTo = true;

        Guard guard = new(this, ReleaseWriter);
        writer = new WeakReference<Guard>(guard);

        return guard;
    }

    /// <summary>
    ///     Try to acquire the resource for reading or writing.
    /// </summary>
    /// <param name="access">The access type to acquire.</param>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    public Guard? TryAcquire(Access access)
    {
        return access switch
        {
            Access.Read => TryAcquireReader(),
            Access.Write => TryAcquireWriter(),
            _ => null
        };
    }

    /// <summary>
    ///     Whether it is currently possible to acquire the resource for certain access.
    /// </summary>
    /// <param name="access">The access type to acquire.</param>
    /// <returns>Whether it is possible to acquire the resource.</returns>
    public Boolean CanAcquire(Access access)
    {
        return access switch
        {
            Access.Read => CanRead,
            Access.Write => CanWrite,
            _ => false
        };
    }

    /// <summary>
    ///     Triggered when either the last reader or only writer releases the resource.
    /// </summary>
    public event EventHandler? Released;
}
