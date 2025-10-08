// <copyright file="Models.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Resources;

/// <summary>
///     All models of the game.
/// </summary>
public class Models : ResourceCatalog
{
    /// <summary>
    ///     Create a new instance of the models catalog.
    /// </summary>
    public Models() : base([
        new ModelLoader(),
        new ModelProvider()
    ]) {}
}
