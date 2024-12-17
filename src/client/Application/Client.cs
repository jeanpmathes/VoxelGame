// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application.Resources;
using VoxelGame.Client.Application.Settings;
using VoxelGame.Client.Inputs;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Core;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Application;

/// <summary>
///     The game window and also the class that represents the running game instance.
/// </summary>
internal partial class Client : Graphics.Core.Client, IPerformanceProvider
{
    private readonly GameParameters parameters;

    private readonly SceneFactory sceneFactory;
    private readonly SceneManager sceneManager;

    private readonly OperationUpdateDispatch operations = new(singleton: true);

    private WindowBehaviour windowBehaviour = null!;

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

        Keybinds = new KeybindManager(Input);

        SizeChanged += OnSizeChanged;
    }

    /// <summary>
    ///     Get the game client instance.
    /// </summary>
    internal static Client Instance { get; private set; } = null!;

    /// <summary>
    ///     Get the keybinds bound for the game.
    /// </summary>
    internal KeybindManager Keybinds { get; }

    internal GeneralSettings Settings { get; }
    internal GraphicsSettings Graphics { get; }

    /// <summary>
    ///     Get the resources of the game.
    /// </summary>
    internal GameResources Resources { get; }

    private Double FPS => windowBehaviour.FPS;
    private Double UPS => windowBehaviour.UPS;

    Double IPerformanceProvider.FPS => FPS;
    Double IPerformanceProvider.UPS => UPS;

    protected override void OnInitialization()
    {
        using (Timer? timer = logger.BeginTimedScoped("Client Load", TimingStyle.Once))
        {
            windowBehaviour = new WindowBehaviour(this);

            LoadingContext loadingContext = new(timer);

            Resources.Load(Graphics.VisualConfiguration, loadingContext);

            IScene startScene = sceneFactory.CreateStartScene(loadingContext.State, parameters.DirectlyLoadedWorldIndex);
            sceneManager.Load(startScene);

            LogFinishedOnLoad(logger);
        }

        // Optional generation of manual.
        ManualBuilder.EmitManual();
    }

    protected override void OnRenderUpdate(Double delta)
    {
        using Timer? timer = logger.BeginTimedScoped("Client Render Update");

        sceneManager.RenderUpdate(delta, timer);
        windowBehaviour.RenderUpdate(delta);
    }

    protected override void OnLogicUpdate(Double delta)
    {
        using Timer? timer = logger.BeginTimedScoped("Client Update");

        using (logger.BeginTimedSubScoped("Client Operations", timer))
        {
            operations.LogicUpdate();
        }

        sceneManager.LogicUpdate(delta, timer);
        windowBehaviour.LogicUpdate(delta);
    }

    protected override void OnDestroy()
    {
        sceneManager.Unload();
        Resources.Dispose();
    }

    protected override Boolean CanClose()
    {
        return sceneManager.CanCloseWindow();
    }

    /// <summary>
    ///     Start a game in a world. A game can only be started when no other game is running.
    /// </summary>
    /// <param name="world">The world to start the game in.</param>
    internal void StartGame(World world)
    {
        IScene gameScene = sceneFactory.CreateGameScene(world);
        sceneManager.Load(gameScene);
    }

    /// <summary>
    ///     Exit the current game.
    /// </summary>
    /// <param name="exitToOS">Whether to exit the complete application or just to the start scene.</param>
    internal void ExitGame(Boolean exitToOS)
    {
        IScene? scene = null;

        if (!exitToOS) scene = sceneFactory.CreateStartScene(resourceLoadingFailure: null, loadWorldDirectly: null);

        sceneManager.Load(scene);

        if (!exitToOS) return;

        LogExitingToOS(logger);

        Close();
    }

    private void OnSizeChanged(Object? sender, SizeChangeEventArgs e)
    {
        LogWindowResized(logger, Size);

        sceneManager.OnResize(Size);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

    [LoggerMessage(EventId = LogID.Client + 0, Level = LogLevel.Information, Message = "Finished client loading")]
    private static partial void LogFinishedOnLoad(ILogger logger);

    [LoggerMessage(EventId = LogID.Client + 1, Level = LogLevel.Information, Message = "Exiting to OS")]
    private static partial void LogExitingToOS(ILogger logger);

    [LoggerMessage(EventId = LogID.Client + 2, Level = LogLevel.Debug, Message = "Window has been resized to: {Size}")]
    private static partial void LogWindowResized(ILogger logger, Vector2i size);

    #endregion

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
            SizeChanged -= OnSizeChanged;

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion DISPOSABLE
}
