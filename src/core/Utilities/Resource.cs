// <copyright file="Resource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    private readonly string name;
    private bool isWrittenTo;
    private int readerCount;

    /// <summary>
    ///     Creates a new resource with the given name.
    /// </summary>
    /// <param name="name"></param>
    public Resource(string name)
    {
        this.name = name;
    }

    /// <summary>
    ///     Get whether there is any reader or writer currently using the resource.
    /// </summary>
    public bool IsAcquired => readerCount > 0 || isWrittenTo;

    /// <summary>
    ///     Get whether it is possible to read from the resource.
    /// </summary>
    public bool CanRead => !isWrittenTo;

    /// <summary>
    ///     Get whether it is possible to write to the resource.
    /// </summary>
    public bool CanWrite => readerCount == 0 && CanRead;

    /// <summary>
    ///     Check if this resource is currently held by a specific guard.
    /// </summary>
    /// <param name="guard">The guard to check.</param>
    /// <param name="access">The access level to check for.</param>
    /// <returns>True if the guard holds the resource with the specified access level.</returns>
    public bool IsHeldBy(Guard guard, Access access)
    {
        return access switch
        {
            Access.Read => guard.IsGuarding(this) && !isWrittenTo,
            Access.Write => guard.IsGuarding(this) && isWrittenTo,
            _ => false
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return name;
    }

    private void ReleaseReader()
    {
        if (readerCount == 0) Debug.Fail("No reader to release.");
        else readerCount--;
    }

    private void ReleaseWriter()
    {
        if (!isWrittenTo) Debug.Fail("No writer to release.");
        else isWrittenTo = false;
    }

    /// <summary>
    ///     Try to acquire the resource for reading.
    /// </summary>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    public Guard? TryAcquireReader()
    {
        if (!ApplicationInformation.Instance.EnsureMainThread("Resource.TryAcquireReader()", this)) return null;
        if (!CanRead) return null;

        readerCount++;

        return new Guard(this, ReleaseReader);
    }

    /// <summary>
    ///     Try to acquire the resource for writing.
    /// </summary>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    public Guard? TryAcquireWriter()
    {
        if (!ApplicationInformation.Instance.EnsureMainThread("Resource.TryAcquireWriter()", this)) return null;
        if (!CanWrite) return null;

        isWrittenTo = true;

        return new Guard(this, ReleaseWriter);
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
    public bool CanAcquire(Access access)
    {
        return access switch
        {
            Access.Read => CanRead,
            Access.Write => CanWrite,
            _ => false
        };
    }
}
