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

            Screen.SetCursor(false, true);

            ui = new GameUserInterface(client, false);

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
                70f,
                0.25f,
                camera,
                new BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.25f, 0.9f, 0.25f)),
                ui);

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();

            counter.ResetUpdate();

            logger.LogInformation("Loaded GameScene");
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

                if (wireframeToggle.Changed)
                {
                    Screen.SetWireFrame(wireframeToggle.State);

                    if (wireframeToggle.State) logger.LogInformation("Enable wireframe mode");
                    else logger.LogInformation("Disable wireframe mode");
                }

                if (uiToggle.Changed) ui.IsHidden = !ui.IsHidden;

                if (escapeButton.Pushed) client.LoadStartScene();
            }
        }

        public void Unload()
        {
            logger.LogInformation("Unloading world");

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support.
    }
}