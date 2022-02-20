// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Scenes;
using VoxelGame.Input;
using VoxelGame.Input.Devices;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
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
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

        private readonly InputManager input;
        private readonly SceneFactory sceneFactory;

        private readonly SceneManager sceneManager;

        private ScreenBehaviour screenBehaviour = null!;

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

            Resources = new GameResources();

            sceneManager = new SceneManager();
            sceneFactory = new SceneFactory(this);

            Load += OnLoad;

            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;

            Closed += OnClosed;

            input = new InputManager(this);
            Keybinds = new KeybindManager(input);
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

        /// <summary>
        ///     Get the resources of the game.
        /// </summary>
        public GameResources Resources { get; }

        /// <summary>
        ///     Get the current game, if there is one.
        /// </summary>
        public Game? CurrentGame { get; private set; }

        private double Time { get; set; }

        internal double FPS => screenBehaviour.FPS;
        internal double UPS => screenBehaviour.UPS;

        double IPerformanceProvider.FPS => FPS;
        double IPerformanceProvider.UPS => UPS;

        private new void OnLoad()
        {
            using (logger.BeginScope("Client OnLoad"))
            {
                Resources.Prepare();

                screenBehaviour = new ScreenBehaviour(this);

                Resources.Load();

                sceneManager.Load(sceneFactory.CreateStartScene());

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

                Resources.Shaders.SetTime((float) Time);

                screenBehaviour.Clear();

                sceneManager.Render((float) e.Time);

                screenBehaviour.Draw(e.Time);

                SwapBuffers();
            }
        }

        private new void OnUpdateFrame(FrameEventArgs e)
        {
            using (logger.BeginScope("UpdateFrame"))
            {
                var deltaTime = (float) MathHelper.Clamp(e.Time, min: 0f, max: 1f);

                input.UpdateState(KeyboardState, MouseState);

                sceneManager.Update(deltaTime);
                screenBehaviour.Update(e.Time);
            }
        }

        private new void OnClosed()
        {
            logger.LogInformation(Events.WindowState, "Closing window");

            sceneManager.Unload();
            Resources.Unload();
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

        /// <summary>
        /// Start a game in a world. A game can only be started when no other game is running.
        /// </summary>
        /// <param name="world">The world to start the game in.</param>
        public void StartGame(ClientWorld world)
        {
            IScene gameScene = sceneFactory.CreateGameScene(world, out Game game);
            sceneManager.Load(gameScene);

            CurrentGame = game;
        }

        /// <summary>
        /// Exit the current game.
        /// </summary>
        public void ExitGame()
        {
            IScene startScene = sceneFactory.CreateStartScene();
            sceneManager.Load(startScene);

            CurrentGame = null;
        }

        public void OnResize(Vector2i size)
        {
            sceneManager.OnResize(size);
        }
    }
}