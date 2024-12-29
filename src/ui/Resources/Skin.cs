// <copyright file="Skin.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Skin;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI.Resources;

/// <summary>
/// Wraps a skin as a resource.
/// </summary>
public sealed class Skin(RID identifier, SkinBase skin) : IResource
{
    /// <summary>
    /// The wrapped skin.
    /// </summary>
    public SkinBase Value { get; } = skin;

    /// <inheritdoc />
    public RID Identifier { get; } = identifier;

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Skin;

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSING
}
