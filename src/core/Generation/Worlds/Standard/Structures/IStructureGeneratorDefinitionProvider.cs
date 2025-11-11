// <copyright file="IStructureGeneratorDefinitionProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.Structures;

/// <summary>
///     Provides structure generator definitions for the world generator.
/// </summary>
public interface IStructureGeneratorDefinitionProvider
{
    /// <summary>
    ///     Get the structure definition by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the structure.</param>
    /// <returns>The structure definition, or a fallback if the structure is not found.</returns>
    public StructureGeneratorDefinition GetStructure(RID identifier);
}
