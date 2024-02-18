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
using VoxelGame.Core.Logic;
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

    private bool isFirstUpdate = true;

    private int? loadWorldDirectly;

    internal StartScene(Application.Client client, ResourceLoadingFailure? resourceLoadingFailure, int? loadWorldDirectly)
    {
        this.client = client;
        this.resourceLoadingFailure = resourceLoadingFailure;
        this.loadWorldDirectly = loadWorldDirectly;

        worldProvider = new WorldProvider(Program.WorldsDirectory);
        worldProvider.WorldActivation += (_, world) => client.StartGame(world);

        List<ISettingsProvider> settingsProviders = new()
        {
            client.Settings,
            Application.Client.Instance.Keybinds,
            client.Graphics
        };

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
    public void Update(double deltaTime)
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
    public void Render(float deltaTime)
    {
        Throw.IfDisposed(disposed);

        ui.Render();
    }

    /// <inheritdoc />
    public void Unload()
    {
        Throw.IfDisposed(disposed);

        // Method intentionally left empty.
    }

    /// <inheritdoc />
    public bool CanCloseWindow()
    {
        return true;
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

        WorldData? data = worldProvider.Worlds.ElementAtOrDefault(index);

        if (data != null)
        {
            logger.LogInformation(Events.Scene, "Loading world at index {Index} directly", index);

            worldProvider.BeginLoadingWorld(data);
        }
        else
        {
            logger.LogError(Events.Scene, "Could not directly-load world at index {Index}, going to main menu", index);
        }
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
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
