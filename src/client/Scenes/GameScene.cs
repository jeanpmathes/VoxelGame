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
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Client.Rendering;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public class GameScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

        private readonly GameUserInterface ui;

        private readonly Client client;

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
        }

        public void Load()
        {
            Game.SetWorld(World);

            // Player setup.
            Camera camera = new Camera(new Vector3());
            Player = new ClientPlayer(70f, 0.25f, camera, new Core.Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.25f, 0.9f, 0.25f)));
            Game.SetPlayer(Player);

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();

            Game.ResetUpdate();

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
                ui.SetUpdateRate(1 / Client.LastRenderDelta, 1 / Client.LastUpdateDelta);

                World.Render();

                ui.Render();
            }
        }

        public void Update(float deltaTime)
        {
            using (logger.BeginScope("GameScene Update"))
            {
                Game.IncrementUpdate();

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
                        GL.LineWidth(1f);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        wireframeMode = false;

                        logger.LogInformation("Disabled wireframe mode.");
                    }
                    else
                    {
                        GL.LineWidth(5f);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        wireframeMode = true;

                        logger.LogInformation("Enabled wireframe mode.");
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
                    Client.LoadStartScene();
                }
            }
        }

        public void Unload()
        {
            logger.LogInformation("Unloading world.");

            try
            {
                World.Save().Wait();
            }
            catch (AggregateException exception)
            {
                logger.LogCritical(LoggingEvents.WorldSavingError, exception.GetBaseException(), "An exception was thrown when saving the world.");
            }

            World.Dispose();
            Player.Dispose();

            World = null!;
            Player = null!;

            Client.InvalidateWorld();
            Client.InvalidatePlayer();
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