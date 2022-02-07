// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>


using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Client.Collections;
using VoxelGame.Client.Console;
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
using VoxelGame.UI.Providers;
using TextureLayout = VoxelGame.Core.Logic.TextureLayout;
#if MANUAL
using System.Globalization;
using VoxelGame.Core;
using Section = VoxelGame.Manual.Section;
using VoxelGame.Manual;
using VoxelGame.Manual.Modifiers;
using VoxelGame.Manual.Utility;
#endif

namespace VoxelGame.Client.Application
{
    /// <summary>
    ///     The game window and also the class that represents the running game instance.
    /// </summary>
    internal class Client : GameWindow, IPerformanceProvider
    {
        private const int DeltaBufferCapacity = 30;
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

        private readonly CommandInvoker commandInvoker;

        private readonly ToggleButton fullscreenToggle;

        private readonly Debug glDebug;

        private readonly InputManager input;

        private readonly CircularTimeBuffer renderDeltaBuffer = new(DeltaBufferCapacity);
        private readonly SceneManager sceneManager;

        private readonly CircularTimeBuffer updateDeltaBuffer = new(DeltaBufferCapacity);

        private Screen screen = null!;

        /// <summary>
        ///     Create a new game instance.
        /// </summary>
        /// <param name="gameWindowSettings">The game window settings.</param>
        /// <param name="nativeWindowSettings">The native window settings.</param>
        /// <param name="graphicsSettings">The graphics settings.</param>
        public Client(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
            GraphicsSettings graphicsSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            Instance = this;

            Settings = new GeneralSettings(Properties.Settings.Default);
            Graphics = graphicsSettings;

            glDebug = new Debug();

            sceneManager = new SceneManager();

            Load += OnLoad;

            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;

            Closed += OnClosed;

            input = new InputManager(this);
            Keybinds = new KeybindManager(input);

            fullscreenToggle = Keybinds.GetToggle(Keybinds.Fullscreen);

            commandInvoker = GameConsole.BuildInvoker();
        }

        /// <summary>
        ///     Get the game client instance.
        /// </summary>
        public static Client Instance { get; private set; } = null!;

        /// <summary>
        ///     Get the keybinds bound for the game.
        /// </summary>
        public KeybindManager Keybinds { get; }

        /// <summary>
        ///     Get the mouse used by the client,
        /// </summary>
        public Mouse Mouse => input.Mouse;

        public GeneralSettings Settings { get; }
        public GraphicsSettings Graphics { get; }

        public ConsoleWrapper Console { get; } = new();

        private double Time { get; set; }

        double IPerformanceProvider.FPS => Fps;
        double IPerformanceProvider.UPS => Ups;

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

                // Shader setup.
                Shaders.Load("Resources/Shaders");

                // Block setup.
                Block.LoadBlocks(BlockTextureArray);

                logger.LogDebug(
                    Events.ResourceLoad,
                    "Texture/Block ratio: {Ratio:F02}",
                    BlockTextureArray.Count / (float) Block.Count);

                // Liquid setup.
                Liquid.LoadLiquids(LiquidTextureArray);

                // Scene setup.
                sceneManager.Load(new StartScene(this));

                logger.LogInformation(Events.ApplicationState, "Finished OnLoad");

#if MANUAL
                // Optional generation of manual.
                GenerateManual();
#endif
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
                var deltaTime = (float) MathHelper.Clamp(e.Time, min: 0f, max: 1f);

                input.UpdateState(KeyboardState, MouseState);

                sceneManager.Update(deltaTime);

                if (IsFocused && fullscreenToggle.Changed) Screen.SetFullscreen(!Instance.IsFullscreen);

                updateDeltaBuffer.Write(e.Time);
            }
        }

        private new void OnClosed()
        {
            logger.LogInformation(Events.WindowState, "Closing window");

            sceneManager.Unload();
            Shaders.Delete();
        }


        [UsedImplicitly]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used in other build type.")]
        private void GenerateManual()
        {
#if MANUAL
            const string path = "./../../../../../../Setup/Resources/Manual";

            Documentation documentation = new(typeof(GameInformation).Assembly);

            Includable controls = new("controls", path);

            controls.CreateSections(
                Keybinds.Binds,
                keybind => Section.Create(keybind.Name)
                    .Text("The key is bound to").Key(keybind.Default).Text("per default.").EndSection());

            controls.Generate();

            Includable blocks = new("blocks", path);

            blocks.CreateSections(
                typeof(Block).GetStaticValues<Block>(documentation),
                ((Block block, string description) s) => Section.Create(s.block.Name)
                    .Text(s.description).NewLine()
                    .BeginList()
                    .Item("ID:").Text(s.block.NamedId, TextStyle.Monospace)
                    .Item("Solid:").Boolean(s.block.IsSolid)
                    .Item("Interactions:").Boolean(s.block.IsInteractable)
                    .Item("Replaceable:").Boolean(s.block.IsReplaceable)
                    .End().EndSection());

            blocks.Generate();

            Includable liquids = new("liquids", path);

            liquids.CreateSections(
                typeof(Liquid).GetStaticValues<Liquid>(documentation),
                ((Liquid liquid, string description) s) => Section.Create(s.liquid.Name)
                    .Text(s.description).NewLine()
                    .BeginList()
                    .Item("ID:").Text(s.liquid.NamedId, TextStyle.Monospace)
                    .Item("Viscosity:").Text(s.liquid.Viscosity.ToString(CultureInfo.InvariantCulture))
                    .Item("Density:").Text(s.liquid.Density.ToString(CultureInfo.InvariantCulture))
                    .End().EndSection());

            liquids.Generate();
#endif
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

        /// <summary>
        ///     Load the game scene.
        /// </summary>
        /// <param name="world">The world to play in.</param>
        public void LoadGameScene(ClientWorld world)
        {
            GameScene gameScene = new(Instance, world, new GameConsole(commandInvoker));

            sceneManager.Load(gameScene);

            Player = gameScene.Player;
        }

        /// <summary>
        ///     Load the start scene.
        /// </summary>
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
