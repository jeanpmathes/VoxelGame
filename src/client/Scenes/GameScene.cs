// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
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

namespace VoxelGame.Client.Scenes
{
    /// <summary>
    ///     The scene that is active when the game is played.
    /// </summary>
    public sealed class GameScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

        private readonly Application.Client client;

        private readonly ToggleButton consoleToggle;

        private readonly UpdateCounter counter;
        private readonly PushButton escapeButton;

        private readonly PushButton screenshotButton;

        private readonly GameUserInterface ui;
        private readonly ToggleButton uiToggle;

        internal GameScene(Application.Client client, ClientWorld world, GameConsole console)
        {
            this.client = client;

            Screen.SetCursor(visible: false, locked: true);

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

            ui.WorldExit += client.ExitGame;

            ui.AnyOverlayOpen += () =>
            {
                Screen.SetOverlayLock();
                Screen.SetCursor(visible: true);
            };

            ui.AnyOverlayClosed += () =>
            {
                Screen.ClearOverlayLock();
                Screen.SetCursor(visible: false, locked: true);
            };

            counter = world.UpdateCounter;

            uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

            screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
            consoleToggle = client.Keybinds.GetToggle(client.Keybinds.Console);
            escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);

            Camera camera = new(new Vector3());

            ClientPlayer player = new(
                world,
                mass: 70f,
                drag: 0.25f,
                camera,
                new BoundingBox(new Vector3(x: 0.5f, y: 1f, z: 0.5f), new Vector3(x: 0.25f, y: 0.9f, z: 0.25f)),
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

            // UI setup.
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
        public void Update(float deltaTime)
        {
            using (logger.BeginScope("GameScene Update"))
            {
                counter.Increment();

                Game.World.Update(deltaTime);

                if (!Screen.IsFocused) // check to see if the window is focused
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
            logger.LogInformation(Events.WorldIO, "Unloading world");

            try
            {
                Game.World.FinishAll().Wait();
                Game.World.Save().Wait();
            }
            catch (AggregateException exception)
            {
                logger.LogCritical(
                    Events.WorldSavingError,
                    exception.GetBaseException(),
                    "Exception occurred while saving world");
            }

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
}
