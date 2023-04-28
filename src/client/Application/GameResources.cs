﻿// <copyright file="GameResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.Extensions.Logging;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.UI;

namespace VoxelGame.Client.Application;

/// <summary>
///     Prepares, loads and offers game resources.
/// </summary>
public class GameResources
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameResources>();

    private readonly Support.Client window;

    /// <summary>
    ///     Create the graphics resources.
    /// </summary>
    public GameResources(Support.Client window)
    {
        this.window = window;
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
    ///     Load the resources. This requires a valid OpenGL context.
    /// </summary>
    public void Load(LoadingContext loadingContext)
    {
        BlockModel.EnableLoading(loadingContext);
        StaticStructure.SetLoadingContext(loadingContext);

        PerformLoading(loadingContext);

        StaticStructure.ClearLoadingContext();
        BlockModel.DisableLoading();
    }

    private void PerformLoading(LoadingContext loadingContext)
    {
        var texParams = TextureParameters.CreateForWorld(Client.Instance);

        using (loadingContext.BeginStep(Events.ResourceLoad, "World Textures"))
        {
            using (loadingContext.BeginStep(Events.ResourceLoad, "Block Textures"))
            {
                BlockTextureArray = new ArrayTexture(Client.Instance,
                    loadingContext,
                    FileSystem.GetResourceDirectory("Textures", "Blocks"),
                    resolution: 32,
                    1 << 13); // todo: use constant here and in vertex data creation
            }

            using (loadingContext.BeginStep(Events.ResourceLoad, "Fluid Textures"))
            {
                FluidTextureArray = new ArrayTexture(Client.Instance,
                    loadingContext,
                    FileSystem.GetResourceDirectory("Textures", "Fluids"),
                    resolution: 32,
                    1 << 11); // todo: use constant here and in vertex data creation
            }
        }

        UIResources.Load(window, loadingContext);

        Shaders = Shaders.Load(FileSystem.GetResourceDirectory("Shaders"), window, loadingContext);

        TextureLayout.SetProviders(BlockTextureArray, FluidTextureArray);
        BlockModel.SetBlockTextureIndexProvider(BlockTextureArray);

        BlockTextureArray.EnableLoading(loadingContext);
        FluidTextureArray.EnableLoading(loadingContext);

        Blocks.Load(BlockTextureArray, loadingContext);

        logger.LogDebug(
            Events.ResourceLoad,
            "Texture/Block ratio: {Ratio:F02}",
            BlockTextureArray.Count / (double) Blocks.Instance.Count);

        Fluids.Load(FluidTextureArray, loadingContext);

        PlayerResources.Load(loadingContext);

        Generator.Prepare(loadingContext);

        BlockTextureArray.DisableLoading();
        FluidTextureArray.DisableLoading();
    }

    /// <summary>
    ///     Unload and free all resources.
    /// </summary>
    public void Unload()
    {
        Shaders.Delete();
        UIResources.Unload();
    }
}
