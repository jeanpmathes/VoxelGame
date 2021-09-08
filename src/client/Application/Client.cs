// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using VoxelGame.Client.Collections;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics;
using VoxelGame.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Devices;
using VoxelGame.Logging;
using TextureLayout = VoxelGame.Core.Logic.TextureLayout;

namespace VoxelGame.Client.Application
{
    internal class Client : GameWindow
    {
        private const int DeltaBufferCapacity = 30;
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

        private readonly InputManager input;
        private readonly SceneManager sceneManager;

        private readonly CircularTimeBuffer renderDeltaBuffer = new(DeltaBufferCapacity);
        private readonly CircularTimeBuffer updateDeltaBuffer = new(DeltaBufferCapacity);

        private readonly ToggleButton fullscreenToggle;

        private readonly Debug glDebug;

        public readonly string screenshotDirectory;
        public readonly string worldsDirectory;

        private Screen screen = null!;

        public Client(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
            string appDataDirectory, string screenshotDirectory) : base(gameWindowSettings, nativeWindowSettings)
        {
            Instance = this;

            unsafe
            {
                WindowPointer = WindowPtr;
            }

            glDebug = new Debug();

            this.screenshotDirectory = screenshotDirectory;

            worldsDirectory = Path.Combine(appDataDirectory, "Worlds");
            Directory.CreateDirectory(worldsDirectory);

            sceneManager = new SceneManager();

            Load += OnLoad;

            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;

            Closed += OnClosed;

            input = new InputManager(this);
            Keybinds = new KeybindManager(input);

            fullscreenToggle = Keybinds.GetToggle(Keybinds.Fullscreen);
        }

        public static Client Instance { get; private set; } = null!;
        public KeybindManager Keybinds { get; }
        public Mouse Mouse => input.Mouse;

        private double Time { get; set; }
        public unsafe Window* WindowPointer { get; }

        private new void OnLoad()
        {
            using (logger.BeginScope("Client OnLoad"))
            {
                // GL debug setup.
                glDebug.Enable();

                // Screen setup.
                screen = new Screen(this);

                // Texture setup.
                BlockTextureArray = new ArrayTexture(
                    "Resources/Textures/Blocks",
                    16,
                    true,
                    TextureUnit.Texture1,
                    TextureUnit.Texture2,
                    TextureUnit.Texture3,
                    TextureUnit.Texture4);

                logger.LogInformation("Block textures loaded");

                LiquidTextureArray = new ArrayTexture("Resources/Textures/Liquids", 16, false, TextureUnit.Texture5);
                logger.LogInformation("Liquid textures loaded");

                TextureLayout.SetProviders(BlockTextureArray, LiquidTextureArray);
                BlockModel.SetBlockTextureIndexProvider(BlockTextureArray);

                // Shader setup.
                Shaders.Load("Resources/Shaders");

                // Block setup.
                Block.LoadBlocks(BlockTextureArray);
                logger.LogDebug("Texture/Block ratio: {Ratio:F02}", BlockTextureArray.Count / (float) Block.Count);

                // Liquid setup.
                Liquid.LoadLiquids(LiquidTextureArray);

                // Scene setup.
                sceneManager.Load(new StartScene(this));

                logger.LogInformation("Finished OnLoad");
            }
        }

        private new void OnRenderFrame(FrameEventArgs e)
        {
            using (logger.BeginScope("RenderFrame"))
            {
                Time += e.Time;

                Shaders.SetTime((float) Time);

                screen.Clear();

                sceneManager.Render((float) e.Time);

                screen.Draw();

                SwapBuffers();

                renderDeltaBuffer.Write(e.Time);
            }
        }

        private new void OnUpdateFrame(FrameEventArgs e)
        {
            using (logger.BeginScope("UpdateFrame"))
            {
                var deltaTime = (float) MathHelper.Clamp(e.Time, 0f, 1f);

                input.UpdateState(KeyboardState, MouseState);

                sceneManager.Update(deltaTime);

                if (IsFocused && fullscreenToggle.Changed) Screen.SetFullscreen(!Instance.IsFullscreen);

                updateDeltaBuffer.Write(e.Time);
            }
        }

        private new void OnClosed()
        {
            logger.LogInformation("Closing window");

            sceneManager.Unload();
        }

        #region STATIC PROPERTIES

        /// <summary>
        ///     Gets the <see cref="ArrayTexture" /> that contains all block textures. It is bound to unit 1, 2, 3, and 4.
        /// </summary>
        public static ArrayTexture BlockTextureArray { get; private set; } = null!;

        /// <summary>
        ///     Gets the <see cref="ArrayTexture" /> that contains all liquid textures. It is bound to unit 5.
        /// </summary>
        public static ArrayTexture LiquidTextureArray { get; private set; } = null!;

        public static ClientPlayer Player { get; private set; } = null!;

        public static double Fps => 1.0 / Instance.renderDeltaBuffer.Average;
        public static double Ups => 1.0 / Instance.updateDeltaBuffer.Average;

        #endregion STATIC PROPERTIES

        #region SCENE MANAGEMENT

        public void LoadGameScene(ClientWorld world)
        {
            GameScene gameScene = new(Instance, world);

            sceneManager.Load(gameScene);

            Player = gameScene.Player;
        }

        public void LoadStartScene()
        {
            sceneManager.Load(new StartScene(Instance));

            Player = null!;
        }

        public void OnResize(Vector2i size)
        {
            sceneManager.OnResize(size);
        }

        #endregion SCENE MANAGEMENT
    }
}
