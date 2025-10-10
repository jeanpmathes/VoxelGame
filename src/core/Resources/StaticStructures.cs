// <copyright file="CoreContent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Resources;

/// <summary>
///     All static structures of the game.
/// </summary>
public class StaticStructures : ResourceCatalog
{
    /// <summary>
    ///     Create an instance of the static structures catalog.
    /// </summary>
    public StaticStructures() : base([
        new StaticStructureLoader(),
        new StaticStructureProvider()
    ]) {}
}
