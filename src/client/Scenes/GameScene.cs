﻿// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors;
using VoxelGame.Client.Actors.Players;
using VoxelGame.Client.Application;
using VoxelGame.Client.Console;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene that is active when the game is played.
/// </summary>
public sealed partial class GameScene : IScene
{
    private readonly ToggleButton consoleToggle;
    private readonly PushButton escapeButton;
    private readonly PushButton unlockMouse;
    private readonly PushButton screenshotButton;

    private readonly GameUserInterface ui;
    private readonly ToggleButton uiToggle;

    private Boolean isMouseUnlockedByUserRequest;

    internal GameScene(Application.Client client, World world)
    {
        Client = client;

        ui = CreateUI(client);
        Game = CreateGame(client, world);

        GameConsole console = new(Game, client.Resources.Commands);

        world.State.Activated += (_, _) =>
        {
            console.OnWorldReady();
        };

        SetUpUI(world, console);

        uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

        screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
        consoleToggle = client.Keybinds.GetToggle(client.Keybinds.Console);
        escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);
        unlockMouse = client.Keybinds.GetPushButton(client.Keybinds.UnlockMouse);
    }

    /// <summary>
    ///     Get whether any overlay is open. If this is the case, game input should be disabled.
    /// </summary>
    public Boolean IsOverlayOpen { get; private set; }

    /// <summary>
    ///     Get whether the game window is focused.
    /// </summary>
    public Boolean IsWindowFocused => Client.IsFocused;

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

        if (ui.Console != null)
            Game.Initialize(new ConsoleWrapper(ui.Console));

        Client.OnFocusChange += OnFocusChanged;

        LogLoadedGameScene(logger);
    }

    /// <inheritdoc />
    public void OnResize(Vector2i size)
    {
        Throw.IfDisposed(disposed);

        ui.Resize(size);
    }

    /// <inheritdoc />
    public void Render(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        using Timer? subTimer = logger.BeginTimedSubScoped("GameScene Render", timer);

        using (logger.BeginTimedSubScoped("GameScene Render Game", subTimer))
        {
            Game.Render();
        }

        using (logger.BeginTimedSubScoped("GameScene Render UI", subTimer))
        {
            RenderUI();
        }
    }

    /// <inheritdoc />
    public void Update(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        using Timer? subTimer = logger.BeginTimedSubScoped("GameScene Update", timer);

        using (logger.BeginTimedSubScoped("GameScene Update UI", subTimer))
        {
            ui.Update();
        }

        using (Timer? gameTimer = logger.BeginTimedSubScoped("GameScene Update Game", subTimer))
        {
            Game.Update(deltaTime, gameTimer);
        }

        if (!Client.IsFocused)
            return;

        if (!IsOverlayOpen)
        {
            if (screenshotButton.Pushed) Client.TakeScreenshot(Program.ScreenshotDirectory);

            if (uiToggle.Changed) ui.ToggleHidden();
        }

        if (unlockMouse.Pushed)
        {
            if (isMouseUnlockedByUserRequest)
            {
                OnOverlayClose();
            }
            else if (!IsOverlayOpen)
            {
                OnOverlayOpen();
                isMouseUnlockedByUserRequest = true;
            }
        }

        if (escapeButton.Pushed)
            ui.HandleEscape();

        if (consoleToggle.Changed)
            ui.ToggleConsole();
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
    public Boolean CanCloseWindow()
    {
        return false;
    }

    private static GameUserInterface CreateUI(Application.Client client)
    {
        return new GameUserInterface(
            client.Input,
            client.Settings,
            client.Resources.UI,
            drawBackground: false);
    }

    private Game CreateGame(Application.Client client, World world)
    {
        Player player = new(
            mass: 70f,
            client.Space.Camera,
            new BoundingVolume(new Vector3d(x: 0.25f, y: 0.9f, z: 0.25f)),
            new VisualInterface(ui, client.Resources),
            this);

        world.AddPlayer(player);

        return new Game(world, player);
    }

    private void SetUpUI(Core.Logic.World world, IConsoleProvider console)
    {
        OnOverlayClose();

        List<SettingsProvider> settingsProviders =
        [
            SettingsProvider.Wrap(Client.Settings),
            SettingsProvider.Wrap(Client.Keybinds)
        ];

        ui.SetSettingsProviders(settingsProviders);
        ui.SetConsoleProvider(console);
        ui.SetPerformanceProvider(Client);

        ui.WorldSave += (_, _) =>
        {
            if (!world.State.IsActive) return;

            world.State.BeginSaving(() =>
            {
                // Nothing to do.
            });
        };

        ui.WorldExit += (_, args) =>
        {
            if (!world.State.IsActive) return;

            world.State.BeginTerminating(() =>
            {
                Client.ExitGame(args.ExitToOS);
            });
        };

        ui.AnyOverlayOpened += (_, _) => OnOverlayOpen();
        ui.AnyOverlayClosed += (_, _) => OnOverlayClose();
    }

    private void OnOverlayClose()
    {
        IsOverlayOpen = false;
        Client.Input.Mouse.SetCursorLock(locked: true);

        isMouseUnlockedByUserRequest = false;
    }

    private void OnOverlayOpen()
    {
        IsOverlayOpen = true;
        Client.Input.Mouse.SetCursorLock(locked: false);

        // The mouse was unlocked, but the user did not explicitly request it.
    }

    private void OnFocusChanged(Object? sender, FocusChangeEventArgs e)
    {
        if (!Client.IsFocused) ui.HandleLossOfFocus();
    }

    private void RenderUI()
    {
        ui.UpdatePerformanceData();
        ui.Render();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

    [LoggerMessage(EventId = Events.SceneChange, Level = LogLevel.Information, Message = "Loaded the game scene")]
    private static partial void LogLoadedGameScene(ILogger logger);

    #endregion LOGGING

    #region IDisposable Graphics.

    private Boolean disposed;

    private void Dispose(Boolean disposing)
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

    #endregion IDisposable Graphics.
}
