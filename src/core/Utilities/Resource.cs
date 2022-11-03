// <copyright file="Resource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

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
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Resource>();

    private readonly string name;
    private bool isWritenTo;
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
    public bool IsAcquired => readerCount > 0 || isWritenTo;

    /// <summary>
    ///     Get whether it is possible to read from the resource.
    /// </summary>
    public bool CanRead => !isWritenTo;

    /// <summary>
    ///     Get whether it is possible to write to the resource.
    /// </summary>
    public bool CanWrite => readerCount == 0 && CanRead;

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
        if (!isWritenTo) Debug.Fail("No writer to release.");
        else isWritenTo = false;
    }

    /// <summary>
    ///     Try to acquire the resource for reading.
    /// </summary>
    /// <returns>The guard that releases the resource when disposed, or null if the resource is not available.</returns>
    public Guard? TryAcquireReader()
    {
        if (!IsMainThread()) return null;
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
        if (!IsMainThread()) return null;
        if (!CanWrite) return null;

        isWritenTo = true;

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

    private bool IsMainThread()
    {
        if (Thread.CurrentThread == ApplicationInformation.Instance.MainThread) return true;

        logger.LogWarning("Attempted to acquire resource '{Resource}' from non-main thread", name);
        Debug.Fail("Attempted to acquire resource from non-main thread.");

        return false;
    }
}
