// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Updates;
using VoxelGame.Input.Actions;
using VoxelGame.Logging;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public class GameScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

        private readonly Application.Client client;

        private readonly UpdateCounter counter;
        private readonly PushButton escapeButton;

        private readonly PushButton screenshotButton;

        private readonly GameUserInterface ui;
        private readonly ToggleButton uiToggle;

        private readonly ToggleButton wireframeToggle;

        internal GameScene(Application.Client client, ClientWorld world)
        {
            this.client = client;

            Screen.SetCursor(visible: false, locked: true);

            ui = new GameUserInterface(client, client.Keybinds.Input.Listener, drawBackground: false);
            ui.WorldExit += client.LoadStartScene;

            ui.MenuOpen += () =>
            {
                Player.LockInput();
                Screen.SetCursor(visible: true, locked: false);
            };

            ui.MenuClose += () =>
            {
                Player.UnlockInput();
                Screen.SetCursor(visible: false, locked: true);
            };

            World = world;
            counter = world.UpdateCounter;

            wireframeToggle = client.Keybinds.GetToggle(client.Keybinds.Wireframe);
            uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

            screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
            escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);
        }

        public ClientWorld World { get; private set; }
        public ClientPlayer Player { get; private set; } = null!;

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

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();

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
                ui.SetUpdateRate(Application.Client.Fps, Application.Client.Ups);

                World.Render();

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

                if (screenshotButton.Pushed) Screen.TakeScreenshot(client.screenshotDirectory);

                if (wireframeToggle.Changed) Screen.SetWireFrame(wireframeToggle.State);

                if (uiToggle.Changed) ui.IsHidden = !ui.IsHidden;

                if (escapeButton.Pushed) ui.OpenInGameMenu();
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
        }

        #region IDisposable Support.

        private bool disposed;

        protected virtual void Dispose(bool disposing)
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

        #endregion IDisposable Support.
    }
}