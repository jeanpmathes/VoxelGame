// <copyright file="GameResources.cs" company="VoxelGame">
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
    ///     Gets the <see cref="TextureBundle" /> that contains all block textures.
    /// </summary>
    private TextureBundle BlockTextures { get; set; } = null!;

    /// <summary>
    ///     Gets the <see cref="TextureBundle" /> that contains all fluid textures.
    /// </summary>
    private TextureBundle FluidTextures { get; set; } = null!;

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
        using (loadingContext.BeginStep(Events.ResourceLoad, "World Textures"))
        {
            using (loadingContext.BeginStep(Events.ResourceLoad, "Block Textures"))
            {
                BlockTextures = TextureBundle.Load(Client.Instance,
                    loadingContext,
                    FileSystem.GetResourceDirectory("Textures", "Blocks"),
                    resolution: 32,
                    1 << 13); // todo: use constant here and in vertex data creation
            }

            using (loadingContext.BeginStep(Events.ResourceLoad, "Fluid Textures"))
            {
                FluidTextures = TextureBundle.Load(Client.Instance,
                    loadingContext,
                    FileSystem.GetResourceDirectory("Textures", "Fluids"),
                    resolution: 32,
                    1 << 11); // todo: use constant here and in vertex data creation
            }
        }

        UIResources.Load(window, loadingContext);

        Shaders = Shaders.Load(FileSystem.GetResourceDirectory("Shaders"), window, (BlockTextures.TextureArray, FluidTextures.TextureArray), loadingContext);

        TextureLayout.SetProviders(BlockTextures, FluidTextures);
        BlockModel.SetBlockTextureIndexProvider(BlockTextures);

        BlockTextures.EnableLoading(loadingContext);
        FluidTextures.EnableLoading(loadingContext);

        Blocks.Load(BlockTextures, loadingContext);

        logger.LogDebug(
            Events.ResourceLoad,
            "Texture/Block ratio: {Ratio:F02}",
            BlockTextures.Count / (double) Blocks.Instance.Count);

        Fluids.Load(FluidTextures, loadingContext);

        PlayerResources.Load(loadingContext);

        Generator.Prepare(loadingContext);

        BlockTextures.DisableLoading();
        FluidTextures.DisableLoading();
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
