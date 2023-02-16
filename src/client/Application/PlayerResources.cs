// <copyright file="PlayerResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;

namespace VoxelGame.Client.Application;

/// <summary>
///     Contains all the resources that are used to render the player and its interface.
/// </summary>
public class PlayerResources
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<PlayerResources>();

    /// <summary>
    ///     The crosshair texture.
    /// </summary>
    public Texture Crosshair { get; private set; } = null!;

    /// <summary>
    ///     Loads all the resources.
    /// </summary>
    public void Load()
    {
        Crosshair = new Texture(
            FileSystem.AccessResourceDirectory("Textures", "UI").GetFile("crosshair.png"),
            TextureUnit.Texture10,
            fallbackResolution: 32);

        logger.LogDebug(Events.ResourceLoad, "Player resources loaded");
    }

    /// <summary>
    ///     Unloads all the resources.
    /// </summary>
    public void Unload()
    {
        Crosshair.Dispose();
    }
}
