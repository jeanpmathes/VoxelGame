﻿// <copyright file="GameResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Console;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Logging;
using VoxelGame.UI;

namespace VoxelGame.Client.Application.Resources;

/// <summary>
///     Prepares, loads and offers game resources.
/// </summary>
public sealed class GameResources : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameResources>();

    private readonly Client client;

    /// <summary>
    ///     Create the graphics resources.
    /// </summary>
    internal GameResources(Client client)
    {
        this.client = client;
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
    public Pipelines Pipelines { get; private set; } = null!;

    /// <summary>
    ///     The player resources.
    /// </summary>
    public PlayerResources Player { get; } = new();

    /// <summary>
    ///     The UI resources.
    /// </summary>
    public UIResources UI { get; } = new();

    /// <summary>
    ///     A collection of all commands and their invokers.
    /// </summary>
    public CommandInvoker Commands { get; private set; } = null!;

    /// <summary>
    ///     Load the resources.
    /// </summary>
    public void Load(VisualConfiguration visuals, LoadingContext loadingContext)
    {
        Throw.IfDisposed(disposed);

        BlockModel.EnableLoading(loadingContext);
        StaticStructure.SetLoadingContext(loadingContext);

        PerformLoading(visuals, loadingContext);

        StaticStructure.ClearLoadingContext();
        BlockModel.DisableLoading();
    }

    private void PerformLoading(VisualConfiguration visuals, LoadingContext loadingContext)
    {
        using (loadingContext.BeginStep(Events.ResourceLoad, "World Textures"))
        {
            using (loadingContext.BeginStep(Events.ResourceLoad, "Block Textures"))
            {
                BlockTextures = TextureBundle.Load(Client.Instance,
                    loadingContext,
                    FileSystem.GetResourceDirectory("Textures", "Blocks"),
                    resolution: 32,
                    Meshing.MaxTextureCount,
                    Image.MipmapAlgorithm.AveragingWithoutTransparency);
            }

            using (loadingContext.BeginStep(Events.ResourceLoad, "Fluid Textures"))
            {
                FluidTextures = TextureBundle.Load(Client.Instance,
                    loadingContext,
                    FileSystem.GetResourceDirectory("Textures", "Fluids"),
                    resolution: 32,
                    Meshing.MaxFluidTextureCount,
                    Image.MipmapAlgorithm.AveragingWithTransparency);
            }
        }

        UI.Load(client, loadingContext);

        Pipelines = Pipelines.Load(FileSystem.GetResourceDirectory("Shaders"), client, TextureBundle.GetTextureSlots(BlockTextures, FluidTextures), visuals, loadingContext);

        BlockModel.SetBlockTextureIndexProvider(BlockTextures);

        BlockTextures.EnableLoading(loadingContext);
        FluidTextures.EnableLoading(loadingContext);

        Blocks.Load(BlockTextures, visuals, loadingContext);

        logger.LogDebug(
            Events.ResourceLoad,
            "Texture/Block ratio: {Ratio:F02}",
            BlockTextures.Count / (Double) Blocks.Instance.Count);

        Fluids.Load(FluidTextures, FluidTextures, loadingContext);

        Player.Load(client, loadingContext);

        Generator.Prepare(loadingContext);

        BlockTextures.DisableLoading();
        FluidTextures.DisableLoading();

        Commands = GameConsole.BuildInvoker();
    }

    #region IDisposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        Pipelines.Dispose();
        UI.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~GameResources()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
