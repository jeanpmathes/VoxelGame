// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    /// <summary>
    ///     The scene the game starts in. It contains the main menu.
    /// </summary>
    public sealed class StartScene : IScene
    {
        private readonly Application.Client client;
        private readonly StartUserInterface ui;

        internal StartScene(Application.Client client)
        {
            this.client = client;
            WorldProvider worldProvider = new(Program.WorldsDirectory);
            worldProvider.WorldActivation += (_, args) => client.StartGame(args);

            List<ISettingsProvider> settingsProviders = new()
            {
                client.Settings,
                Application.Client.Instance.Keybinds,
                client.Graphics
            };

            ui = new StartUserInterface(
                client,
                client.Keybinds.Input.Listener,
                worldProvider,
                settingsProviders,
                drawBackground: true);
        }

        /// <inheritdoc />
        public void Load()
        {
            Screen.SetCursor(visible: true);
            Screen.SetWireframe(wireframe: false);
            Screen.EnterUIDrawMode();

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();
            ui.SetExitAction(() => client.Close());
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc />
        public void OnResize(Vector2i size)
        {
            ui.Resize(size);
        }

        /// <inheritdoc />
        public void Render(float deltaTime)
        {
            ui.Render();
        }

        /// <inheritdoc />
        public void Unload()
        {
            // Method intentionally left empty.
        }

        #region IDisposable Support

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
        ///     Disposes of the scene.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~StartScene()
        {
            Dispose(disposing: false);
        }

        #endregion IDisposable Support
    }
}
