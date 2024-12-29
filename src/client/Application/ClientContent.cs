// <copyright file="ClientResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Client.Console;
using VoxelGame.Client.Resources;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Resources;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI.Resources;

namespace VoxelGame.Client.Application;

/// <summary>
/// The content (resources) of the client.
/// </summary>
public class ClientContent : ResourceCatalog
{
    /// <summary>
    /// Create a new instance of the client resources catalog.
    /// </summary>
    public ClientContent() : base([
        new Textures(),
        new TextureInfoProvider(),
        new EngineLoader(),
        new Models(),
        new CoreContent(),
        new PlayerContent(),
        new CommandLoader(),
        new UserInterfaceContent(),
    ]) {}
}
