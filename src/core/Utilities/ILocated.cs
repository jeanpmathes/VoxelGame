// <copyright file="ILocated.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A resource which allows determining its file location just with a name.
///     This means the resource defines a path and file extension.
/// </summary>
public interface ILocated
{
    /// <summary>
    ///     The resource-relative path to the directory containing all resources of this type.
    /// </summary>
    public static abstract String[] Path { get; }

    /// <summary>
    ///     The file extension associated with this resource type, without the leading dot.
    /// </summary>
    public static abstract String FileExtension { get; }
}
