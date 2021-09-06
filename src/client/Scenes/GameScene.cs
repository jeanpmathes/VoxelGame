﻿// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Updates;
using VoxelGame.Input.Actions;
using VoxelGame.Logging;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public class GameScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

        private readonly GameUserInterface ui;

        private readonly Application.Client client;

        private readonly UpdateCounter counter;

        public ClientWorld World { get; private set; }
        public ClientPlayer Player { get; private set; } = null!;

        private readonly ToggleButton wireframeToggle;
        private readonly ToggleButton uiToggle;

        private readonly PushButton screenshotButton;
        private readonly PushButton escapeButton;

        internal GameScene(Application.Client client, ClientWorld world)
        {
            this.client = client;

            Screen.SetCursor(visible: false, locked: true);

            ui = new GameUserInterface(client, false);

            World = world;
            counter = world.UpdateCounter;

            wireframeToggle = client.Keybinds.GetToggle(client.Keybinds.Wireframe);
            uiToggle = client.Keybinds.GetToggle(client.Keybinds.UI);

            screenshotButton = client.Keybinds.GetPushButton(client.Keybinds.Screenshot);
            escapeButton = client.Keybinds.GetPushButton(client.Keybinds.Escape);
        }

        public void Load()
        {
            // Player setup.
            Camera camera = new Camera(new Vector3());

            Player = new ClientPlayer(
                World,
                70f,
                0.25f,
                camera,
                new Core.Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.25f, 0.9f, 0.25f)),
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
                {
                    return;
                }

                if (screenshotButton.Pushed)
                {
                    Screen.TakeScreenshot(client.screenshotDirectory);
                }

                if (wireframeToggle.Changed)
                {
                    Screen.SetWireFrame(wireframeToggle.State);

                    logger.LogInformation(
                        wireframeToggle.State
                            ? "Enabled wire-frame mode."
                            : "Disabled wire-frame mode.");
                }

                if (uiToggle.Changed)
                {
                    ui.IsHidden = !ui.IsHidden;
                }

                if (escapeButton.Pushed)
                {
                    client.LoadStartScene();
                }
            }
        }

        public void Unload()
        {
            logger.LogInformation("Unloading world.");

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
                    "An exception was thrown when saving the world.");
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
                if (disposing)
                {
                    ui.Dispose();
                }

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