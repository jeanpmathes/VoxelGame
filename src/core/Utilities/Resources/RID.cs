// <copyright file="RID.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;

namespace VoxelGame.Core.Utilities.Resources;

#pragma warning disable S101

/// <summary>
///     Identifies a resource.
///     <c>RID</c> stands for <c>Resource ID</c>.
/// </summary>
public readonly struct RID : IEquatable<RID>
{
    private static readonly String pathPrefix = $"res:{System.IO.Path.DirectorySeparatorChar}{System.IO.Path.DirectorySeparatorChar}";

    private readonly String? identifier;

    /// <summary>
    ///     Get the virtual resource identifier.
    ///     It is used for resources that are not loaded during the loading process.
    ///     Instead, they are created on the fly.
    /// </summary>
    public static RID Virtual => new();

    /// <summary>
    ///     Get a resource identifier for a file.
    /// </summary>
    /// <param name="name">The name of the file, without the file extension.</param>
    /// <typeparam name="T">The type of the resource, providing the directory path and file extension.</typeparam>
    /// <returns>The resource identifier.</returns>
    public static RID File<T>(String name) where T : ILocated
    {
        return Path(FileSystem.GetResourceDirectory(T.Path).GetFile(FileSystem.GetResourceFileName<T>(name)));
    }

    /// <summary>
    ///     Get a resource identifier for any path.
    /// </summary>
    /// <param name="info">The path to the resource.</param>
    /// <returns>The resource identifier.</returns>
    public static RID Path(FileSystemInfo info)
    {
        return new RID($"{pathPrefix}{info.GetResourceRelativePath()}");
    }

    /// <summary>
    ///     Get a resource identifier for a named resource.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <returns>The resource identifier.</returns>
    public static RID Named<T>(String name) where T : IResource
    {
        return new RID(Reflections.GetDecoratedName<T>("Resource", name));
    }

    private RID(String identifier)
    {
        this.identifier = identifier;
    }

    /// <summary>
    ///     Get this resource identifier as a string usable as an instance name in <see cref="Reflections.GetDecoratedName" />.
    ///     Not valid for virtual resource identifiers, will return <c>null</c> in that case.
    /// </summary>
    public String? Instance => identifier != null ? $"Of<{identifier}>" : null;

    /// <inheritdoc />
    public override String ToString()
    {
        return identifier ?? "<virtual>";
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(RID other)
    {
        return identifier == other.identifier;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is RID other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(identifier);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(RID left, RID right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(RID left, RID right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY
}
