// <copyright file="PlayerResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Graphics.Objects;

namespace VoxelGame.Client.Application;

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
    public void Load(LoadingContext loadingContext)
    {
        using (loadingContext.BeginStep(Events.ResourceLoad, "Player"))
        {
            // todo: load crosshair here
            /*
            Crosshair = new Texture(loadingContext,
                FileSystem.GetResourceDirectory("Textures", "UI").GetFile("crosshair.png"),
                TextureUnit.Texture10,
                fallbackResolution: 32);
            */
        }
    }

    /// <summary>
    ///     Unloads all the resources.
    /// </summary>
    public void Unload()
    {
        Crosshair.Dispose();
    }
}

