// <copyright file="Resource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.App;

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
///     Controls read and write access to a resource.
///     It allows multiple readers or a single writer to access the resource at the same time.
/// </summary>
public sealed class RW
{
    private readonly String name;
    private Int32 readerCount;

    /// <summary>
    ///     Creates a new reader-writer control.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    public RW(String name)
    {
        this.name = name;
    }

    /// <summary>
    ///     Get whether there is any reader or writer currently using the resource.
    /// </summary>
    public Boolean IsAcquired => readerCount > 0 || IsWrittenTo;

    /// <summary>
    ///     Get whether the resource is currently written to.
    /// </summary>
    public Boolean IsWrittenTo { get; private set; }

    /// <summary>
    ///     Get whether the resource is currently read from.
    /// </summary>
    public Boolean IsReadFrom => readerCount > 0;

    /// <summary>
    ///     Get whether it is possible to read from the resource.
    /// </summary>
    private Boolean CanRead => !IsWrittenTo;

    /// <summary>
    ///     Get whether it is possible to write to the resource.
    /// </summary>
    private Boolean CanWrite => readerCount == 0 && CanRead;

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
            Access.Read => guard.IsGuarding(this) && !IsWrittenTo,
            Access.Write => guard.IsGuarding(this) && IsWrittenTo,
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
    }

    private void ReleaseWriter()
    {
        if (!IsWrittenTo) Debug.Fail("No writer to release.");
        else IsWrittenTo = false;

        Released?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Try to acquire the resource for reading.
    /// </summary>
    /// <param name="source">The source of the request.</param>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    private Guard? TryAcquireReader(String source)
    {
        Application.ThrowIfNotOnMainThread(this);

        if (!CanRead) return null;

        readerCount++;

        return new Guard(this, source, ReleaseReader);
    }

    /// <summary>
    ///     Try to acquire the resource for writing.
    /// </summary>
    /// <param name="source">The source of the request.</param>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    private Guard? TryAcquireWriter(String source)
    {
        Application.ThrowIfNotOnMainThread(this);

        if (!CanWrite) return null;

        IsWrittenTo = true;

        return new Guard(this, source, ReleaseWriter);
    }

    /// <summary>
    ///     Try to acquire the resource for reading or writing.
    /// </summary>
    /// <param name="access">The access type to acquire.</param>
    /// <param name="source">The source of the request.</param>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    public Guard? TryAcquire(Access access, String source)
    {
        return access switch
        {
            Access.Read => TryAcquireReader(source),
            Access.Write => TryAcquireWriter(source),
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
