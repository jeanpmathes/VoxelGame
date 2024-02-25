// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Console;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Input.Actions;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene that is active when the game is played.
/// </summary>
public sealed class GameScene : IScene
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

    private readonly ToggleButton consoleToggle;

    private readonly PushButton escapeButton;

    private readonly PushButton screenshotButton;

    private readonly GameUserInterface ui;
    private readonly ToggleButton uiToggle;

    internal GameScene(Application.Client client, World world, IConsoleProvider console)
    {
        void OnOverlayClose()
        {
            IsOverlayOpen = false;
            client.Input.Mouse.SetCursorLock(locked: true);
        }

        void OnOverlayOpen()
        {
            IsOverlayOpen = true;
            client.Input.Mouse.SetCursorLock(locked: false);
        }

        OnOverlayClose();

        ui = new GameUserInterface(
            client.Input,
            client.Settings,
            client.Resources.UI,
            drawBackground: false);

        List<ISettingsProvider> settingsProviders = new()
        {
            client.Settings,
            Application.Client.Instance.Keybinds
        };

        ui.SetSettingsProviders(settingsProviders);
        ui.SetConsoleProvider(console);
        ui.SetPerformanceProvider(client);

        ui.WorldExit += (_, args) =>
        {
            if (world.IsActive)
                world.BeginDeactivating(() => client.ExitGame(args.ExitToOS));
        };

        ui.AnyOverlayOpen += (_, _) => OnOverlayOpen();
        ui.AnyOverlayClosed += (_, _) => OnOverlayClose();

        uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

        screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
        consoleToggle = client.Keybinds.GetToggle(client.Keybinds.Console);
        escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);

        Player player = new(
            world,
            mass: 70f,
            client.Space.Camera,
            new BoundingVolume(new Vector3d(x: 0.25f, y: 0.9f, z: 0.25f)),
            ui,
            client.Resources,
            this);

        world.AddPlayer(player);

        world.StateChanged += (_, _) =>
        {
            if (world.IsActive)
                console.OnWorldReady();
        };

        Game = new Game(world, player);
        Client = client;
    }

    /// <summary>
    ///     Get whether any overlay is open. If this is the case, game input should be disabled.
    /// </summary>
    public bool IsOverlayOpen { get; private set; }

    /// <summary>
    ///     Get whether the game window is focused.
    /// </summary>
    public bool IsWindowFocused => Client.IsFocused;

    /// <summary>
    ///     Get the game played in this scene.
    /// </summary>
    public Game Game { get; private set; }

    private Application.Client Client { get; }

    /// <inheritdoc />
    public void Load()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(Game != null, "Scene has been unloaded.");

        ui.SetPlayerDataProvider(Game.Player);

        ui.Load();
        ui.Resize(Client.Size);

        ui.CreateControl();
        Game.Initialize(new ConsoleWrapper(ui.Console!));

        Client.OnFocusChange += OnFocusChanged;

        logger.LogInformation(Events.SceneChange, "Loaded GameScene");
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

        using (logger.BeginScope("GameScene Render"))
        {
            Game.Render();
            RenderUI();
        }
    }

    /// <inheritdoc />
    public void Update(double deltaTime)
    {
        Throw.IfDisposed(disposed);

        using (logger.BeginScope("GameScene Update"))
        {
            ui.Update();

            Game.Update(deltaTime);

            if (!Client.IsFocused)
                return;

            if (!IsOverlayOpen)
            {
                if (screenshotButton.Pushed) Client.TakeScreenshot(Program.ScreenshotDirectory);

                if (uiToggle.Changed) ui.ToggleHidden();
            }

            if (escapeButton.Pushed) ui.HandleEscape();

            if (consoleToggle.Changed) ui.ToggleConsole();
        }
    }

    /// <inheritdoc />
    public void Unload()
    {
        Throw.IfDisposed(disposed);

        Client.OnFocusChange -= OnFocusChanged;

        Game.Dispose();
        Game = null!;
    }

    /// <inheritdoc />
    public bool CanCloseWindow()
    {
        return false;
    }

    private void OnFocusChanged(object? sender, FocusChangeEventArgs e)
    {
        if (!Client.IsFocused) ui.HandleLossOfFocus();
    }

    private void RenderUI()
    {
        ui.UpdatePerformanceData();
        ui.Render();
    }

    #region IDisposable Support.

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing) ui.Dispose();

        disposed = true;
    }

    /// <summary>
    ///     Dispose of the scene.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~GameScene()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support.
}
