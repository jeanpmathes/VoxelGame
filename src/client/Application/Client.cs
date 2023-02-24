// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Utilities;
using VoxelGame.Input;
using VoxelGame.Input.Devices;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Application;

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
    internal Client(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
        GraphicsSettings graphicsSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        Instance = this;

        Settings = new GeneralSettings(Properties.Settings.Default);
        Graphics = graphicsSettings;

        Resources = new GameResources(this);

        sceneManager = new SceneManager();
        sceneFactory = new SceneFactory(this);

        Load += OnLoad;

        RenderFrame += OnRenderFrame;
        UpdateFrame += OnUpdateFrame;

        Closing += OnClosing;

        input = new InputManager(this);
        Keybinds = new KeybindManager(input);
    }

    /// <summary>
    ///     Get the game client instance.
    /// </summary>
    internal static Client Instance { get; private set; } = null!;

    /// <summary>
    ///     Get the keybinds bound for the game.
    /// </summary>
    internal KeybindManager Keybinds { get; }

    /// <summary>
    ///     Get the mouse used by the client,
    /// </summary>
    internal Mouse Mouse => input.Mouse;

    internal GeneralSettings Settings { get; }
    internal GraphicsSettings Graphics { get; }

    /// <summary>
    ///     Get the resources of the game.
    /// </summary>
    internal GameResources Resources { get; }

    /// <summary>
    ///     Get the current game, if there is one.
    /// </summary>
    internal Game? CurrentGame { get; private set; }

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

            LoadingContext loadingContext = new();

            Resources.Load(loadingContext);

            sceneManager.Load(sceneFactory.CreateStartScene(loadingContext.State));

            logger.LogInformation(Events.ApplicationState, "Finished OnLoad");

            // Optional generation of manual.
            ManualBuilder.EmitManual();
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
            double deltaTime = MathHelper.Clamp(e.Time, min: 0f, max: 1f);

            input.UpdateState(KeyboardState, MouseState);

            sceneManager.Update(deltaTime);
            screenBehaviour.Update(e.Time);
        }
    }

    private new void OnClosing(CancelEventArgs e)
    {
        logger.LogInformation(Events.WindowState, "Closing window");

        sceneManager.Unload();
        Resources.Unload();
    }

    /// <summary>
    ///     Start a game in a world. A game can only be started when no other game is running.
    /// </summary>
    /// <param name="world">The world to start the game in.</param>
    internal void StartGame(World world)
    {
        IScene gameScene = sceneFactory.CreateGameScene(world, out Game game);
        sceneManager.Load(gameScene);

        CurrentGame = game;
    }

    /// <summary>
    ///     Exit the current game.
    /// </summary>
    internal void ExitGame()
    {
        IScene startScene = sceneFactory.CreateStartScene(resourceLoadingFailure: null);
        sceneManager.Load(startScene);

        CurrentGame = null;
    }

    internal void OnResize(Vector2i size)
    {
        sceneManager.OnResize(size);
    }
}


