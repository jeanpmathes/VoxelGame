// <copyright file="PlayerResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Application.Resources;

/// <summary>
///     Contains all the resources that are used to render the player and its interface.
/// </summary>
public class PlayerResources
{
    /// <summary>
    ///     The crosshair texture.
    /// </summary>
    public Texture Crosshair { get; private set; } = null!;

    /// <summary>
    ///     Loads all the resources.
    /// </summary>
    public void Load(Support.Core.Client client, ILoadingContext loadingContext)
    {
        using (loadingContext.BeginStep("Player"))
        {
            Crosshair = Texture.Load(
                client,
                FileSystem.GetResourceDirectory("Textures", "UI").GetFile("crosshair.png"),
                loadingContext,
                fallbackResolution: 32);
        }
    }
}
