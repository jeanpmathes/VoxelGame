// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application.Worlds;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene the game starts in. It contains the main menu.
/// </summary>
public sealed partial class StartScene : IScene
{
    private readonly Application.Client client;

    private readonly ResourceLoadingIssueReport? resourceLoadingIssueReport;
    private readonly StartUserInterface ui;

    private readonly WorldProvider worldProvider;

    private Boolean isFirstUpdate = true;

    private Int32? loadWorldDirectly;

    internal StartScene(Application.Client client, UserInterfaceResources uiResources, ResourceLoadingIssueReport? resourceLoadingIssueReport, Int32? loadWorldDirectly)
    {
        this.client = client;
        this.resourceLoadingIssueReport = resourceLoadingIssueReport;
        this.loadWorldDirectly = loadWorldDirectly;

        worldProvider = new WorldProvider(Program.WorldsDirectory);
        worldProvider.WorldActivation += (_, world) => client.StartGame(world);

        List<SettingsProvider> settingsProviders =
        [
            SettingsProvider.Wrap(client.Settings),
            SettingsProvider.Wrap(Application.Client.Instance.Keybinds),
            SettingsProvider.Wrap(client.Graphics)
        ];

        ui = new StartUserInterface(
            client.Input,
            client.Settings,
            worldProvider,
            settingsProviders,
            uiResources,
            drawBackground: true);
    }

    /// <inheritdoc />
    public void Load()
    {
        Throw.IfDisposed(disposed);

        client.Input.Mouse.SetCursorLock(locked: false);

        ui.Load();
        ui.Resize(client.Size);

        ui.CreateControl();
        ui.SetExitAction(() => client.Close());

        if (resourceLoadingIssueReport == null) return;

        ui.PresentResourceLoadingFailure(resourceLoadingIssueReport.Report, resourceLoadingIssueReport.AnyErrors);

        if (loadWorldDirectly is null) return;

        LogResourceLoadingFailurePreventsDirectWorldLoading(logger);
        loadWorldDirectly = null;
    }

    /// <inheritdoc />
    public void RenderUpdate(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        ui.RenderUpdate();
    }

    /// <inheritdoc />
    public void LogicUpdate(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        if (isFirstUpdate)
        {
            LoadWorldDirectlyIfRequested();
            isFirstUpdate = false;
        }

        ui.LogicUpdate();
    }

    /// <inheritdoc />
    public void OnResize(Vector2i size)
    {
        Throw.IfDisposed(disposed);

        ui.Resize(size);
    }

    /// <inheritdoc />
    public void Unload()
    {
        Throw.IfDisposed(disposed);

        // Method intentionally left empty.
    }

    /// <inheritdoc />
    public Boolean CanCloseWindow()
    {
        return ui.IsSafeToClose;
    }

    private void LoadWorldDirectlyIfRequested()
    {
        if (loadWorldDirectly is not {} index) return;

        Exception? exception = worldProvider.Refresh().WaitForCompletion();

        if (exception != null)
        {
            LogCouldNotRefreshWorldsToDirectlyLoadWorld(logger, exception, index);

            return;
        }

        IWorldProvider.IWorldInfo? info = worldProvider.Worlds.ElementAtOrDefault(index);

        if (info != null)
        {
            LogLoadingWorldDirectly(logger, index);

            worldProvider.BeginLoadingWorld(info);
        }
        else
        {
            LogCouldNotDirectlyLoadWorld(logger, index);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<StartScene>();

    [LoggerMessage(EventId = LogID.StartScene + 0, Level = LogLevel.Warning, Message = "Resource loading failure prevents direct world loading, going to main menu")]
    private static partial void LogResourceLoadingFailurePreventsDirectWorldLoading(ILogger logger);

    [LoggerMessage(EventId = LogID.StartScene + 1, Level = LogLevel.Error, Message = "Could not refresh worlds to directly-load world at index {Index}, going to main menu")]
    private static partial void LogCouldNotRefreshWorldsToDirectlyLoadWorld(ILogger logger, Exception exception, Int32 index);

    [LoggerMessage(EventId = LogID.StartScene + 2, Level = LogLevel.Information, Message = "Loading world at index {Index} directly")]
    private static partial void LogLoadingWorldDirectly(ILogger logger, Int32 index);

    [LoggerMessage(EventId = LogID.StartScene + 3, Level = LogLevel.Error, Message = "Could not directly-load world at index {Index}, going to main menu")]
    private static partial void LogCouldNotDirectlyLoadWorld(ILogger logger, Int32 index);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) ui.Dispose();

        disposed = true;
    }

    /// <summary>
    ///     Disposes of the scene.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~StartScene()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
