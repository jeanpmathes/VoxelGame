// <copyright file="ErrorResource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     Proxy resource that represents resource that could not be loaded.
/// </summary>
/// <param name="type">The type of the resource.</param>
/// <param name="identifier">The identifier of the resource.</param>
/// <param name="issue">The error that occurred during loading.</param>
public sealed class MissingResource(ResourceType type, RID identifier, ResourceIssue issue) : IResource
{
    /// <inheritdoc />
    public RID Identifier => identifier;

    /// <inheritdoc />
    public ResourceType Type => type;

    /// <inheritdoc />
    public ResourceIssue Issue => issue;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE
}
