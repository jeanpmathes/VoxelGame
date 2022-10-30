// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Console;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Updates;
using VoxelGame.Input.Actions;
using VoxelGame.Logging;
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

    private readonly UpdateCounter counter;
    private readonly PushButton escapeButton;

    private readonly PushButton screenshotButton;

    private readonly GameUserInterface ui;
    private readonly ToggleButton uiToggle;

    internal GameScene(Application.Client client, ClientWorld world, IConsoleProvider console)
    {
        void OnOverlayClose()
        {
            Screen.ClearOverlayLock();
            Screen.SetCursor(locked: true);
        }

        void OnOverlayOpen()
        {
            Screen.SetOverlayLock();
            Screen.SetCursor(locked: false);
        }

        OnOverlayClose();

        ui = new GameUserInterface(
            client,
            client.Keybinds.Input.Listener,
            drawBackground: false);

        List<ISettingsProvider> settingsProviders = new()
        {
            client.Settings,
            Application.Client.Instance.Keybinds
        };

        ui.SetSettingsProviders(settingsProviders);
        ui.SetConsoleProvider(console);
        ui.SetPerformanceProvider(client);

        ui.WorldExit += (_, _) => world.BeginDeactivating(client.ExitGame);

        ui.AnyOverlayOpen += (_, _) => OnOverlayOpen();
        ui.AnyOverlayClosed += (_, _) => OnOverlayClose();

        counter = world.UpdateCounter;

        uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

        screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
        consoleToggle = client.Keybinds.GetToggle(client.Keybinds.Console);
        escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);

        Camera camera = new(new Vector3d());

        ClientPlayer player = new(
            world,
            mass: 70f,
            camera,
            new BoundingVolume(new Vector3d(x: 0.25f, y: 0.9f, z: 0.25f)),
            ui);

        world.AddPlayer(player);

        Game = new Game(world, player);
    }

    /// <summary>
    ///     Get the game played in this scene.
    /// </summary>
    public Game Game { get; private set; }

    /// <inheritdoc />
    public void Load()
    {
        Debug.Assert(Game != null, "Scene has been unloaded.");

        ui.SetPlayerDataProvider(Game.Player);

        ui.Load();
        ui.Resize(Screen.Size);

        ui.CreateControl();
        Game.InitializeConsole(new ConsoleWrapper(ui.Console!));

        counter.Reset();

        logger.LogInformation(Events.SceneChange, "Loaded GameScene");
    }

    /// <inheritdoc />
    public void OnResize(Vector2i size)
    {
        ui.Resize(size);
    }

    /// <inheritdoc />
    public void Render(float deltaTime)
    {
        using (logger.BeginScope("GameScene Render"))
        {
            Screen.EnterGameDrawMode();
            RenderGame();

            Screen.EnterUIDrawMode();
            RenderUI();
        }
    }

    /// <inheritdoc />
    public void Update(double deltaTime)
    {
        using (logger.BeginScope("GameScene Update"))
        {
            counter.Increment();

            Game.World.Update(deltaTime);

            if (!Screen.IsFocused)
                return;

            if (!Screen.IsOverlayLockActive)
            {
                if (screenshotButton.Pushed) Screen.TakeScreenshot(Program.ScreenshotDirectory);

                if (uiToggle.Changed) ui.IsHidden = !ui.IsHidden;
            }

            if (escapeButton.Pushed) ui.DoEscape();

            if (consoleToggle.Changed) ui.ToggleConsole();
        }
    }

    /// <inheritdoc />
    public void Unload()
    {
        Game.Dispose();
        Game = null!;
    }

    private void RenderGame()
    {
        Game.World.Render();
    }

    private void RenderUI()
    {
        Game.Player.RenderOverlays();

        ui.UpdatePerformanceData();
        ui.Render();
    }

    #region IDisposable Support.

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
