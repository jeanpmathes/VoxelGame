// <copyright file="GameResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics;
using VoxelGame.Logging;
using VoxelGame.UI;
using TextureLayout = VoxelGame.Core.Visuals.TextureLayout;

namespace VoxelGame.Client.Application;

/// <summary>
///     Prepares, loads and offers game resources.
/// </summary>
public class GameResources
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameResources>();

    private readonly Debug glDebug;

    private bool prepared;

    /// <summary>
    ///     Create the graphics resources.
    /// </summary>
    public GameResources()
    {
        glDebug = new Debug();
    }

    /// <summary>
    ///     Gets the <see cref="ArrayTexture" /> that contains all block textures. It is bound to unit 1, 2, 3, and 4.
    /// </summary>
    public ArrayTexture BlockTextureArray { get; private set; } = null!;

    /// <summary>
    ///     Gets the <see cref="ArrayTexture" /> that contains all fluid textures. It is bound to unit 5.
    /// </summary>
    public ArrayTexture FluidTextureArray { get; private set; } = null!;

    /// <summary>
    ///     Get the shaders of the game.
    /// </summary>
    public Shaders Shaders { get; private set; } = null!;

    /// <summary>
    ///     The player resources.
    /// </summary>
    public PlayerResources PlayerResources { get; } = new();

    /// <summary>
    ///     The UI resources.
    /// </summary>
    public UIResources UIResources { get; } = new();

    /// <summary>
    ///     Prepare resource loading and initialization. This requires a valid OpenGL context.
    /// </summary>
    public void Prepare()
    {
        System.Diagnostics.Debug.Assert(!prepared);

        glDebug.Enable();

        prepared = true;
    }

    /// <summary>
    ///     Load the resources. This requires a valid OpenGL context.
    /// </summary>
    public void Load()
    {
        System.Diagnostics.Debug.Assert(prepared);

        var texParams = TextureParameters.CreateForWorld(Client.Instance);

        BlockTextureArray = new ArrayTexture(
            FileSystem.AccessResourceDirectory("Textures", "Blocks"),
            resolution: 32,
            useCustomMipmapGeneration: true,
            texParams,
            TextureUnit.Texture1,
            TextureUnit.Texture2,
            TextureUnit.Texture3,
            TextureUnit.Texture4);

        logger.LogInformation(Events.ResourceLoad, "Block textures loaded");

        FluidTextureArray = new ArrayTexture(
            FileSystem.AccessResourceDirectory("Textures", "Fluids"),
            resolution: 32,
            useCustomMipmapGeneration: false,
            texParams,
            TextureUnit.Texture5);

        logger.LogInformation(Events.ResourceLoad, "Fluid textures loaded");

        TextureLayout.SetProviders(BlockTextureArray, FluidTextureArray);
        BlockModel.SetBlockTextureIndexProvider(BlockTextureArray);

        Shaders = Shaders.Load(FileSystem.AccessResourceDirectory("Shaders"));

        Blocks.Load(BlockTextureArray);

        logger.LogDebug(
            Events.ResourceLoad,
            "Texture/Block ratio: {Ratio:F02}",
            BlockTextureArray.Count / (double) Blocks.Instance.Count);

        Fluids.Load(FluidTextureArray);

        PlayerResources.Load();
        UIResources.Load();

        Generator.Prepare();
    }

    /// <summary>
    ///     Unload and free all resources.
    /// </summary>
    public void Unload()
    {
        Shaders.Delete();

        BlockTextureArray.Dispose();
        FluidTextureArray.Dispose();

        PlayerResources.Unload();
        UIResources.Unload();
    }
}

