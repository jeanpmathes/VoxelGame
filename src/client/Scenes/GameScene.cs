// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using VoxelGame.Client.Rendering;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core.Updates;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public class GameScene : IScene
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<GameScene>();

        private readonly GameUserInterface ui;

        private readonly Client client;

        private readonly UpdateCounter counter;

        public ClientWorld World { get; private set; }
        public ClientPlayer Player { get; private set; } = null!;

        private bool wireframeMode;
        private bool hasReleasesWireframeKey = true;

        private bool hasReleasedScreenshotKey = true;

        private bool hasReleasedUIKey = true;

        internal GameScene(Client client, ClientWorld world)
        {
            this.client = client;

            Screen.SetCursor(visible: false, tracked: true);

            ui = new GameUserInterface(client, false);

            World = world;
            counter = world.UpdateCounter;
        }

        public void Load()
        {
            // Player setup.
            Camera camera = new Camera(new Vector3());
            Player = new ClientPlayer(World, 70f, 0.25f, camera, new Core.Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.25f, 0.9f, 0.25f)), ui);

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();

            counter.ResetUpdate();

            Logger.LogInformation("Loaded GameScene");
        }

        public void OnResize(Vector2i size)
        {
            ui.Resize(size);
        }

        public void Render(float deltaTime)
        {
            using (Logger.BeginScope("GameScene Render"))
            {
                ui.SetUpdateRate(Client.Fps, Client.Ups);

                World.Render();

                ui.Render();
            }
        }

        public void Update(float deltaTime)
        {
            using (Logger.BeginScope("GameScene Update"))
            {
                counter.IncrementUpdate();

                World.Update(deltaTime);

                if (!Screen.IsFocused) // check to see if the window is focused
                {
                    return;
                }

                KeyboardState input = Client.Keyboard;

                if (hasReleasedScreenshotKey && input.IsKeyDown(Key.F12))
                {
                    hasReleasedScreenshotKey = false;

                    Screen.TakeScreenshot(client.ScreenshotDirectory);
                }
                else if (input.IsKeyUp(Key.F12))
                {
                    hasReleasedScreenshotKey = true;
                }

                if (hasReleasesWireframeKey && input.IsKeyDown(Key.K))
                {
                    hasReleasesWireframeKey = false;

                    if (wireframeMode)
                    {
                        Screen.SetWireFrame(false);
                        wireframeMode = false;

                        Logger.LogInformation("Disabled wire-frame mode.");
                    }
                    else
                    {
                        Screen.SetWireFrame(true);
                        wireframeMode = true;

                        Logger.LogInformation("Enabled wire-frame mode.");
                    }
                }
                else if (input.IsKeyUp(Key.K))
                {
                    hasReleasesWireframeKey = true;
                }

                if (hasReleasedUIKey && input.IsKeyDown(Key.J))
                {
                    hasReleasedUIKey = false;

                    ui.IsHidden = !ui.IsHidden;
                }
                else if (input.IsKeyUp(Key.J))
                {
                    hasReleasedUIKey = true;
                }

                if (input.IsKeyDown(Key.Escape))
                {
                    client.LoadStartScene();
                }
            }
        }

        public void Unload()
        {
            Logger.LogInformation("Unloading world.");

            try
            {
                World.FinishAll().Wait();
                World.Save().Wait();
            }
            catch (AggregateException exception)
            {
                Logger.LogCritical(LoggingEvents.WorldSavingError, exception.GetBaseException(), "An exception was thrown when saving the world.");
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