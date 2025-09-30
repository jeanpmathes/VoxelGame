// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application.Components;
using VoxelGame.Client.Application.Settings;
using VoxelGame.Client.Inputs;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Resources;
using VoxelGame.Client.Scenes;
using VoxelGame.Client.Visuals;
using VoxelGame.Client.Visuals.Textures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Core;
using VoxelGame.Logging;
using VoxelGame.UI.Resources;

namespace VoxelGame.Client.Application;

/// <summary>
///     The game window and also the class that represents the running game instance.
/// </summary>
public sealed partial class Client : Graphics.Core.Client
{
    private readonly GameParameters parameters;

    private readonly SceneFactory sceneFactory;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly SceneManager sceneManager;

    private Boolean isExitingToOS;

    /// <summary>
    ///     Create a new client instance.
    /// </summary>
    /// <param name="windowSettings">The window settings.</param>
    /// <param name="graphicsSettings">The graphics settings.</param>
    /// <param name="parameters">The parameters, passed from the command line.</param>
    /// <param name="version">The version of the client.</param>
    internal Client(WindowSettings windowSettings, GraphicsSettings graphicsSettings, GameParameters parameters, Version version) : base(windowSettings, version)
    {
        this.parameters = parameters;

        sceneManager = AddComponent<SceneManager>();
        sceneFactory = new SceneFactory(this);

        AddComponent<SceneOperationDispatch>();
        AddComponent<GlobalOperationDispatch>();

        Settings = new GeneralSettings(Properties.Settings.Default);
        Graphics = graphicsSettings;

        Graphics.CreateSettings(this);

        Keybinds = new KeybindManager(Settings, Input);

        SizeChanged += OnSizeChanged;
    }

    /// <summary>
    ///     Get the keybinds bound for the game.
    /// </summary>
    internal KeybindManager Keybinds { get; }

    internal GeneralSettings Settings { get; }
    internal GraphicsSettings Graphics { get; }

    private IResourceContext? UIResources { get; set; }
    private IResourceContext? MainResources { get; set; }

    /// <inheritdoc />
    protected override void OnInitialization(Timer? timer)
    {
        AddComponent<FullscreenToggle, Client>();
        AddComponent<CycleTracker>();

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

        Scene? startScene = sceneFactory.CreateStartScene(issueReport, parameters.DirectlyLoadedWorldIndex);

        if (startScene != null)
        {
            if (MainResources.Get<Engine>() is {} engine)
                Visuals.Graphics.Instance.Initialize(engine);

            sceneManager.BeginLoad(startScene);
        }

        LogFinishedOnLoad(logger);

        // Optional generation of manual.
        ManualBuilder.EmitManual(this);
    }

    /// <inheritdoc />
    protected override void OnLogicUpdate(Double delta, Timer? timer)
    {
        if (sceneManager.IsActive)
            return;

        ExitToOS();
    }

    /// <inheritdoc />
    protected override void OnDestroy(Timer? timer)
    {
        sceneManager.UnloadImmediately();

        UIResources?.Dispose();
        MainResources?.Dispose();
    }

    /// <inheritdoc />
    protected override Boolean CanClose()
    {
        return sceneManager.CanCloseWindow();
    }

    /// <summary>
    ///     Start a session in the given world. A session can only be started when no other session is running.
    /// </summary>
    /// <param name="world">The world to start the session in.</param>
    internal void StartSession(World world)
    {
        Scene? gameScene = sceneFactory.CreateSessionScene(world);

        if (gameScene != null)
            sceneManager.BeginLoad(gameScene);
    }

    /// <summary>
    ///     Exit the current session.
    /// </summary>
    /// <param name="exitToOS">Whether to exit the complete application or just to the start scene.</param>
    internal void ExitGame(Boolean exitToOS)
    {
        Scene? scene = null;

        if (!exitToOS)
            scene = sceneFactory.CreateStartScene(resourceLoadingIssueReport: null, loadWorldDirectly: null);

        if (scene != null)
            sceneManager.BeginLoad(scene);
        else
            sceneManager.BeginUnload();
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

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            SizeChanged -= OnSizeChanged;
        }

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion DISPOSABLE
}
