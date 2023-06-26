﻿// <copyright file="StartScene.cs" company="VoxelGame">
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
using VoxelGame.Client.Rendering;
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

    private readonly int? loadWorldDirectly;

    private readonly ResourceLoadingFailure? resourceLoadingFailure;
    private readonly StartUserInterface ui;

    private readonly WorldProvider worldProvider;

    private bool isFirstUpdate = true;

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
        Screen.SetCursor(locked: false);
        Screen.SetWireframe(wireframe: false);
        Screen.EnterUIDrawMode();

        ui.Load();
        ui.Resize(Screen.Size);

        ui.CreateControl();
        ui.SetExitAction(() => client.Close());

        if (resourceLoadingFailure != null) ui.PresentResourceLoadingFailure(resourceLoadingFailure.MissingResources, resourceLoadingFailure.IsCritical);
    }

    /// <inheritdoc />
    public void Update(double deltaTime)
    {
        if (!isFirstUpdate) return;

        isFirstUpdate = false;

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
