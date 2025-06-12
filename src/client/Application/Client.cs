// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application.Settings;
using VoxelGame.Client.Inputs;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Resources;
using VoxelGame.Client.Scenes;
using VoxelGame.Client.Visuals;
using VoxelGame.Client.Visuals.Textures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Core;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Resources;

namespace VoxelGame.Client.Application;

/// <summary>
///     The game window and also the class that represents the running game instance.
/// </summary>
internal partial class Client : Graphics.Core.Client, IPerformanceProvider
{
    private readonly GameParameters parameters;

    private readonly SceneFactory sceneFactory;
    private readonly SceneManager sceneManager;

    private readonly OperationUpdateDispatch sceneOperations = new(singleton: true);

    private WindowBehaviour windowBehaviour = null!;

    private Boolean isExitingToOS;

    /// <summary>
    ///     Create a new game instance.
    /// </summary>
    /// <param name="windowSettings">The window settings.</param>
    /// <param name="graphicsSettings">The graphics settings.</param>
    /// <param name="parameters">The parameters, passed from the command line.</param>
    internal Client(WindowSettings windowSettings, GraphicsSettings graphicsSettings, GameParameters parameters) : base(windowSettings)
    {
        this.parameters = parameters;

        Settings = new GeneralSettings(Properties.Settings.Default);
        Graphics = graphicsSettings;

        Graphics.CreateSettings(this);

        sceneManager = new SceneManager(sceneOperations);
        sceneFactory = new SceneFactory(this);

        Keybinds = new KeybindManager(Settings, Input);

        SizeChanged += OnSizeChanged;
    }

    /// <summary>
    ///     Get the keybinds bound for the game.
    /// </summary>
    internal KeybindManager Keybinds { get; }

    internal GeneralSettings Settings { get; }
    internal GraphicsSettings Graphics { get; }

    private Double FPS => windowBehaviour.FPS;
    private Double UPS => windowBehaviour.UPS;

    private IResourceContext? UIResources { get; set; }
    private IResourceContext? MainResources { get; set; }

    /// <summary>
    ///     Get an update dispatch which does not cancel and complete operations on scene change.
    ///     Using this dispatch is necessary when operations should continue even when the scene changes.
    ///     Otherwise, using the default dispatch is recommended.
    /// </summary>
    internal OperationUpdateDispatch ClientUpdateDispatch { get; } = new();

    Double IPerformanceProvider.FPS => FPS;
    Double IPerformanceProvider.UPS => UPS;

    protected override void OnInitialization()
    {
        using (Timer? timer = logger.BeginTimedScoped("Client Load", TimingStyle.Once))
        {
            windowBehaviour = new WindowBehaviour(this);

            ResourceCatalogLoader loader = new();

            loader.AddToEnvironment(this);
            loader.AddToEnvironment(Graphics.VisualConfiguration);

            (UIResources, ResourceLoadingIssueReport? uiIssues) = loader.Load(new UserInterfaceRequirements(), timer);

            if (uiIssues is {AnyErrors: true})
            {
                LogFailedToLoadUIResources(logger);

                ExitToOS();

                return;
            }

            loader.AddToEnvironment(UIResources);

            (MainResources, ResourceLoadingIssueReport? mainIssues) = loader.Load(new ClientContent(), timer);
            ResourceLoadingIssueReport? issueReport = ResourceLoadingIssueReport.Merge("Resources", uiIssues, mainIssues);

            sceneFactory.InitializeResources(MainResources);

            if (MainResources.Get<TextureBundle>(Textures.BlockID) is {} blockTextures)
                LogTextureBlockRatio(logger, blockTextures.Count / (Double) Blocks.Instance.Count);

            IScene? startScene = sceneFactory.CreateStartScene(issueReport, parameters.DirectlyLoadedWorldIndex);

            if (startScene != null)
            {
                if (MainResources.Get<Engine>() is {} engine)
                    Visuals.Graphics.Initialize(engine);

                sceneManager.Load(startScene);
            }

            LogFinishedOnLoad(logger);
        }

        // Optional generation of manual.
        ManualBuilder.EmitManual(this);
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

        if (!sceneManager.IsInScene)
        {
            ExitToOS();

            return;
        }

        using (logger.BeginTimedSubScoped("Client Operations", timer))
        {
            sceneOperations.LogicUpdate();
            ClientUpdateDispatch.LogicUpdate();
        }

        sceneManager.LogicUpdate(delta, timer);
        windowBehaviour.LogicUpdate(delta);
    }

    protected override void OnDestroy()
    {
        sceneManager.Unload();

        UIResources?.Dispose();
        MainResources?.Dispose();
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
        IScene? gameScene = sceneFactory.CreateGameScene(world);

        if (gameScene != null)
            sceneManager.Load(gameScene);
    }

    /// <summary>
    ///     Exit the current game.
    /// </summary>
    /// <param name="exitToOS">Whether to exit the complete application or just to the start scene.</param>
    internal void ExitGame(Boolean exitToOS)
    {
        IScene? scene = null;

        if (!exitToOS)
            scene = sceneFactory.CreateStartScene(resourceLoadingIssueReport: null, loadWorldDirectly: null);

        if (scene != null)
            sceneManager.Load(scene);
        else
            sceneManager.Unload();
    }

    private void ExitToOS()
    {
        if (isExitingToOS)
            return;

        LogExitingToOS(logger);

        isExitingToOS = true;

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

    [LoggerMessage(EventId = LogID.Client + 3, Level = LogLevel.Debug, Message = "Texture/Block ratio: {Ratio:F02}")]
    private static partial void LogTextureBlockRatio(ILogger logger, Double ratio);

    [LoggerMessage(EventId = LogID.Client + 4, Level = LogLevel.Critical, Message = "Failed to load required UI resources, exiting")]
    private static partial void LogFailedToLoadUIResources(ILogger logger);

    #endregion

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            SizeChanged -= OnSizeChanged;

            ClientUpdateDispatch.CompleteAll();
        }

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion DISPOSABLE
}
