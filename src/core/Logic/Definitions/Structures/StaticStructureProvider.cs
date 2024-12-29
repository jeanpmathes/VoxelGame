// <copyright file="StaticStructureProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
/// Provides all loaded static structures.
/// </summary>
public class StaticStructureProvider() : ResourceProvider<StaticStructure>(StaticStructure.CreateFallback, structure => structure), IStructureProvider
{
    /// <inheritdoc />
    public Structure GetStructure(RID identifier)
    {
        return GetResource(identifier);
    }
}
