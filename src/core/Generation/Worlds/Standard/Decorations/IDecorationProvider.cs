// <copyright file="IDecorationProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Provides decorations for the world generator.
/// </summary>
public interface IDecorationProvider
{
    /// <summary>
    ///     Get a decoration by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the decoration.</param>
    /// <returns>The decoration, or a fallback decoration if the decoration is not found.</returns>
    Decoration GetDecoration(RID identifier);
}
