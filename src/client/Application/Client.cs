// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Input;
using VoxelGame.Support.Input.Devices;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Application;

/// <summary>
///     The game window and also the class that represents the running game instance.
/// </summary>
internal class Client : Support.Core.Client, IPerformanceProvider
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

    private readonly InputManager input;

    private readonly GameParameters parameters;
    private readonly SceneFactory sceneFactory;

    private readonly SceneManager sceneManager;

    private ScreenBehaviour screenBehaviour = null!;

    /// <summary>
    ///     Create a new game instance.
    /// </summary>
    /// <param name="windowSettings">The window settings.</param>
    /// <param name="graphicsSettings">The graphics settings.</param>
    /// <param name="parameters">The parameters, passed from the command line.</param>
    internal Client(WindowSettings windowSettings, GraphicsSettings graphicsSettings, GameParameters parameters) : base(windowSettings)
    {
        Instance = this;
        this.parameters = parameters;

        Settings = new GeneralSettings(Properties.Settings.Default);
        Graphics = graphicsSettings;

        Resources = new GameResources(this);

        sceneManager = new SceneManager();
        sceneFactory = new SceneFactory(this);

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

    internal double FPS => screenBehaviour.FPS;
    internal double UPS => screenBehaviour.UPS;

    double IPerformanceProvider.FPS => FPS;
    double IPerformanceProvider.UPS => UPS;

    protected override void OnInit()
    {
        using (logger.BeginScope("Client OnLoad"))
        {
            screenBehaviour = new ScreenBehaviour(this);

            LoadingContext loadingContext = new();

            Resources.Load(Graphics.VisualConfiguration, loadingContext);

            IScene startScene = sceneFactory.CreateStartScene(loadingContext.State, parameters.DirectlyLoadedWorldIndex);
            sceneManager.Load(startScene);

            logger.LogInformation(Events.ApplicationState, "Finished OnLoad");

            // Optional generation of manual.
            ManualBuilder.EmitManual();
        }
    }

    protected override void OnRender(double delta)
    {
        using (logger.BeginScope("RenderFrame"))
        {
            // Resources.Shaders.SetTime((float) Time); todo: check whether this is still needed
            sceneManager.Render((float) delta);
            screenBehaviour.Draw(delta);
        }
    }

    protected override void OnUpdate(double delta)
    {
        using (logger.BeginScope("UpdateFrame"))
        {
            double deltaTime = MathHelper.Clamp(delta, min: 0f, max: 1f);

            input.UpdateState(KeyState);

            sceneManager.Update(deltaTime);
            screenBehaviour.Update(delta);
        }
    }

    protected override void OnDestroy()
    {
        logger.LogInformation(Events.WindowState, "Closing window");

        sceneManager.Unload();
        Resources.Unload();
    }

    protected override bool CanClose()
    {
        return sceneManager.CanCloseWindow();
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
        IScene startScene = sceneFactory.CreateStartScene(resourceLoadingFailure: null, loadWorldDirectly: null);
        sceneManager.Load(startScene);

        CurrentGame = null;
    }

    /// <inheritdoc />
    protected override void OnResize(Vector2i size)
    {
        Resources.Pipelines.UpdateOrthographicProjection();

        logger.LogDebug(Events.WindowState, "Window has been resized to: {Size}", size);

        sceneManager.OnResize(size);
    }
}
