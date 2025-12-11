// <copyright file="IStructureProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     Provides structures for given resource identifiers.
/// </summary>
public interface IStructureProvider : IResourceProvider
{
    /// <summary>
    ///     Get the structure for the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the structure.</param>
    /// <returns>The structure, or a fallback structure if the structure is not found.</returns>
    Structure GetStructure(RID identifier);
}
