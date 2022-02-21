﻿// <copyright file="GameResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics;
using VoxelGame.Logging;
using TextureLayout = VoxelGame.Core.Logic.TextureLayout;

namespace VoxelGame.Client.Application
{
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
        ///     Gets the <see cref="ArrayTexture" /> that contains all liquid textures. It is bound to unit 5.
        /// </summary>
        public ArrayTexture LiquidTextureArray { get; private set; } = null!;

        /// <summary>
        ///     Get the shaders of the game.
        /// </summary>
        public Shaders Shaders { get; private set; } = null!;

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

            BlockTextureArray = new ArrayTexture(
                "Resources/Textures/Blocks",
                resolution: 16,
                useCustomMipmapGeneration: true,
                TextureUnit.Texture1,
                TextureUnit.Texture2,
                TextureUnit.Texture3,
                TextureUnit.Texture4);

            logger.LogInformation(Events.ResourceLoad, "Block textures loaded");

            LiquidTextureArray = new ArrayTexture(
                "Resources/Textures/Liquids",
                resolution: 16,
                useCustomMipmapGeneration: false,
                TextureUnit.Texture5);

            logger.LogInformation(Events.ResourceLoad, "Liquid textures loaded");

            TextureLayout.SetProviders(BlockTextureArray, LiquidTextureArray);
            BlockModel.SetBlockTextureIndexProvider(BlockTextureArray);

            Shaders = Shaders.Load("Resources/Shaders");

            // Block setup.
            Block.LoadBlocks(BlockTextureArray);

            logger.LogDebug(
                Events.ResourceLoad,
                "Texture/Block ratio: {Ratio:F02}",
                BlockTextureArray.Count / (float) Block.Count);

            // Liquid setup.
            Liquid.LoadLiquids(LiquidTextureArray);
        }

        /// <summary>
        ///     Unload and free all resources.
        /// </summary>
        public void Unload()
        {
            Shaders.Delete();

            BlockTextureArray.Dispose();
            LiquidTextureArray.Dispose();
        }
    }
}