// <copyright file="SessionScene.cs" company="VoxelGame">
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
using VoxelGame.Client.Application.Components;
using VoxelGame.Client.Console;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Sessions;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene that is active when a session is played.
/// </summary>
public sealed partial class SessionScene : IScene
{
    private readonly ToggleButton consoleToggle;
    private readonly PushButton escapeButton;
    private readonly PushButton unlockMouse;
    private readonly PushButton screenshotButton;

    private readonly GameUserInterface ui;
    private readonly ToggleButton uiToggle;

    private Boolean isMouseUnlockedByUserRequest;

    internal SessionScene(Application.Client client, World world, CommandInvoker commands, UserInterfaceResources uiResources, Engine engine)
    {
        Client = client;

        ui = CreateUI(client, uiResources);
        Session = CreateSession(client.Space.Camera, world, engine);

        SessionConsole console = new(Session, commands);

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
    ///     Get the session played in this scene.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    ///     Get the client that this scene is part of.
    /// </summary>
    internal Application.Client Client { get; }

    /// <inheritdoc />
    public void Load()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(Session != null);

        ui.SetPlayerDataProvider(Session.Player);

        ui.Load();
        ui.Resize(Client.Size);

        ui.CreateControl();

        Client.FocusChanged += OnFocusChanged;

        LogLoadedGameScene(logger);
    }

    /// <inheritdoc />
    public void OnResize(Vector2i size)
    {
        Throw.IfDisposed(disposed);

        ui.Resize(size);
    }

    /// <inheritdoc />
    public void RenderUpdate(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        using Timer? subTimer = logger.BeginTimedSubScoped("SessionScene RenderUpdate", timer);

        using (logger.BeginTimedSubScoped("SessionScene RenderUpdate Session", subTimer))
        {
            Session.RenderUpdate(deltaTime, subTimer);
        }

        using (logger.BeginTimedSubScoped("SessionScene RenderUpdate UI", subTimer))
        {
            RenderUpdateUI();
        }
    }

    /// <inheritdoc />
    public void LogicUpdate(Double deltaTime, Timer? timer)
    {
        Throw.IfDisposed(disposed);

        using Timer? subTimer = logger.BeginTimedSubScoped("SessionScene LogicUpdate", timer);

        using (logger.BeginTimedSubScoped("SessionScene LogicUpdate UI", subTimer))
        {
            ui.LogicUpdate();
        }

        using (Timer? gameTimer = logger.BeginTimedSubScoped("SessionScene LogicUpdate Session", subTimer))
        {
            Session.LogicUpdate(deltaTime, gameTimer);
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

        Client.FocusChanged -= OnFocusChanged;

        Session.Dispose();
        Session = null!;
    }

    /// <inheritdoc />
    public Boolean CanCloseWindow()
    {
        return false;
    }

    private static GameUserInterface CreateUI(Application.Client client, UserInterfaceResources uiResources)
    {
        return new GameUserInterface(
            client.Input,
            client.Settings,
            uiResources,
            drawBackground: false);
    }

    private Session CreateSession(Camera camera, World world, Engine engine)
    {
        Player player = new(
            mass: 70f,
            camera,
            new BoundingVolume(new Vector3d(x: 0.25f, y: 0.9f, z: 0.25f)),
            new VisualInterface(Client.Keybinds, ui, engine),
            this);

        world.AddComponent<LocalPlayerHook, Player>(player);

        return new Session(world, player);
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
        ui.SetPerformanceProvider(Client.GetRequiredComponent<CycleTracker>());

        ui.WorldSave += (_, _) =>
        {
            if (!world.State.IsActive) return;

            world.State.BeginSaving();
        };

        ui.WorldExit += (_, args) =>
        {
            if (!world.State.IsActive) return;

            world.State.BeginTerminating()?.Then(() => Client.ExitGame(args.ExitToOS));
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

    private void RenderUpdateUI()
    {
        ui.UpdatePerformanceData();
        ui.RenderUpdate();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SessionScene>();

    [LoggerMessage(EventId = LogID.GameScene + 0, Level = LogLevel.Information, Message = "Loaded the session scene")]
    private static partial void LogLoadedGameScene(ILogger logger);

    #endregion LOGGING

    #region DISPOSING

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
    ~SessionScene()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSING
}
