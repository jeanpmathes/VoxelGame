// <copyright file="CoreContent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Generation;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Resources;

/// <summary>
///     All content of the core project.
/// </summary>
public class CoreContent : ResourceCatalog
{
    /// <summary>
    ///     Create a new instance of the core content catalog.
    /// </summary>
    public CoreContent() : base([
        new BlockLoader(),
        new FluidLoader(),
        new StaticStructures(),
        new Generators()
    ]) {}
}
