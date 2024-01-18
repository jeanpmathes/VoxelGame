// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
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
            client.Keybinds.Input.Listener,
            worldProvider,
            settingsProviders,
            client.Resources.UIResources,
            drawBackground: true);
    }

    /// <inheritdoc />
    public void Load()
    {
        client.Mouse.SetCursorLock(locked: false);

        ui.Load();
        ui.Resize(client.Size);

        ui.CreateControl();
        ui.SetExitAction(() => client.Close());

        if (resourceLoadingFailure == null) return;

        ui.PresentResourceLoadingFailure(resourceLoadingFailure.MissingResources, resourceLoadingFailure.IsCritical);

        if (loadWorldDirectly is null) return;

        logger.LogWarning("Resource loading failure prevents direct world loading, going to main menu");
        loadWorldDirectly = null;
    }

    /// <inheritdoc />
    public void Update(double deltaTime)
    {
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
        ui.Resize(size);
    }

    /// <inheritdoc />
    public void Render(float deltaTime)
    {
        ui.Render();
    }

    /// <inheritdoc />
    public void Unload()
    {
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

        worldProvider.Refresh();
        (WorldInformation info, DirectoryInfo path) world = worldProvider.Worlds.ElementAtOrDefault(index);

        if (world != default((WorldInformation, DirectoryInfo)))
        {
            logger.LogInformation("Loading world at index {Index} directly", index);

            worldProvider.LoadWorld(world.info, world.path);
        }
        else
        {
            logger.LogError("Could not directly-load world at index {Index}, going to main menu", index);
        }
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing) ui.Dispose();

            disposed = true;
        }
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
