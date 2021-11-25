// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
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

        private readonly ToggleButton wireframeToggle;

        internal GameScene(Application.Client client, ClientWorld world, GameConsole console)
        {
            this.client = client;

            Player = null!;

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

            ui.WorldExit += client.LoadStartScene;

            ui.AnyOverlayOpen += () =>
            {
                Screen.SetOverlayLock();
                Screen.SetCursor(visible: true, locked: false);
            };

            ui.AnyOverlayClosed += () =>
            {
                Screen.ClearOverlayLock();
                Screen.SetCursor(visible: false, locked: true);
            };

            World = world;
            counter = world.UpdateCounter;

            wireframeToggle = client.Keybinds.GetToggle(client.Keybinds.Wireframe);
            uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

            screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
            consoleToggle = client.Keybinds.GetToggle(client.Keybinds.Console);
            escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);

            wireframeToggle.Clear();
        }

        public ClientWorld World { get; private set; }
        public ClientPlayer Player { get; private set; }

        public void Load()
        {
            // Player setup.
            Camera camera = new(new Vector3());

            Player = new ClientPlayer(
                World,
                mass: 70f,
                drag: 0.25f,
                camera,
                new BoundingBox(new Vector3(x: 0.5f, y: 1f, z: 0.5f), new Vector3(x: 0.25f, y: 0.9f, z: 0.25f)),
                ui);

            ui.SetPlayerDataProvider(Player);

            // UI setup.
            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();
            client.Console.SetInterface(ui.Console!);

            counter.ResetUpdate();

            logger.LogInformation(Events.SceneChange, "Loaded GameScene");
        }

        public void OnResize(Vector2i size)
        {
            ui.Resize(size);
        }

        public void Render(float deltaTime)
        {
            using (logger.BeginScope("GameScene Render"))
            {
                Screen.EnterGameDrawMode();
                World.Render();

                Screen.EnterUIDrawMode();
                ui.UpdatePerformanceData();
                ui.Render();
            }
        }

        public void Update(float deltaTime)
        {
            using (logger.BeginScope("GameScene Update"))
            {
                counter.IncrementUpdate();

                World.Update(deltaTime);

                if (!Screen.IsFocused) // check to see if the window is focused
                    return;

                if (!Screen.IsOverlayLockActive)
                {
                    if (screenshotButton.Pushed) Screen.TakeScreenshot(Program.ScreenshotDirectory);

                    if (wireframeToggle.Changed) Screen.SetWireframe(wireframeToggle.State);

                    if (uiToggle.Changed) ui.IsHidden = !ui.IsHidden;
                }

                if (escapeButton.Pushed) ui.DoEscape();

                if (consoleToggle.Changed) ui.ToggleConsole();
            }
        }

        public void Unload()
        {
            logger.LogInformation(Events.WorldIO, "Unloading world");

            try
            {
                World.FinishAll().Wait();
                World.Save().Wait();
            }
            catch (AggregateException exception)
            {
                logger.LogCritical(
                    Events.WorldSavingError,
                    exception.GetBaseException(),
                    "Exception occurred while saving world");
            }

            World.Dispose();
            Player.Dispose();

            World = null!;
            Player = null!;

            client.Console.ClearInterface();
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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~GameScene()
        {
            Dispose(disposing: false);
        }

        #endregion IDisposable Support.
    }
}
