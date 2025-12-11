// <copyright file="IResource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     A resource can be loaded by a <see cref="IResourceLoader" />.
/// </summary>
public interface IResource : IDisposable
{
    /// <summary>
    ///     A justification string to suppress <c>CA2213</c> warnings.
    /// </summary>
    const String ResourcesOwnedByContext = "Resources are owned by the context and should not be disposed manually.";

    /// <summary>
    ///     An identifier for the resource.
    /// </summary>
    RID Identifier { get; }

    /// <summary>
    ///     The type of this resource.
    /// </summary>
    ResourceType Type { get; }

    /// <summary>
    ///     Get an error that occurred during loading, or <c>null</c> if no error occurred.
    ///     If this is set, the resource is considered invalid and will not be added to the context.
    /// </summary>
    ResourceIssue? Issue => null;
}
