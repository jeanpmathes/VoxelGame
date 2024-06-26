﻿// <copyright file="StartScene.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene the game starts in. It contains the main menu.
/// </summary>
public sealed class StartScene : IScene
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<StartScene>();

    private readonly Application.Client client;

    private readonly ResourceLoadingFailure? resourceLoadingFailure;
    private readonly StartUserInterface ui;

    private readonly WorldProvider worldProvider;

    private Boolean isFirstUpdate = true;

    private Int32? loadWorldDirectly;

    internal StartScene(Application.Client client, ResourceLoadingFailure? resourceLoadingFailure, Int32? loadWorldDirectly)
    {
        this.client = client;
        this.resourceLoadingFailure = resourceLoadingFailure;
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
            client.Resources.UI,
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

        if (resourceLoadingFailure == null) return;

        ui.PresentResourceLoadingFailure(resourceLoadingFailure.MissingResources, resourceLoadingFailure.IsCritical);

        if (loadWorldDirectly is null) return;

        logger.LogWarning(Events.Scene, "Resource loading failure prevents direct world loading, going to main menu");
        loadWorldDirectly = null;
    }

    /// <inheritdoc />
    public void Render(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        ui.Render();
    }

    /// <inheritdoc />
    public void Update(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        if (isFirstUpdate)
        {
            DoFirstUpdate();
            isFirstUpdate = false;
        }

        ui.Update();
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

    private void DoFirstUpdate()
    {
        if (loadWorldDirectly is not {} index) return;

        Exception? exception = worldProvider.Refresh().WaitForCompletion();

        if (exception != null)
        {
            logger.LogError(Events.Scene, exception, "Could not refresh worlds to directly-load world at index {Index}, going to main menu", index);

            return;
        }

        IWorldProvider.IWorldInfo? info = worldProvider.Worlds.ElementAtOrDefault(index);

        if (info != null)
        {
            logger.LogInformation(Events.Scene, "Loading world at index {Index} directly", index);

            worldProvider.BeginLoadingWorld(info);
        }
        else
        {
            logger.LogError(Events.Scene, "Could not directly-load world at index {Index}, going to main menu", index);
        }
    }

    #region IDisposable Support

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

    #endregion IDisposable Support
}
