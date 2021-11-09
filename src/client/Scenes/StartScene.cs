// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public sealed class StartScene : IScene
    {
        private readonly Application.Client client;
        private readonly StartUserInterface ui;

        internal StartScene(Application.Client client)
        {
            this.client = client;
            WorldProvider worldProvider = new(Program.WorldsDirectory);
            worldProvider.WorldActivation += client.LoadGameScene;

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

        public void Load()
        {
            Screen.SetCursor(visible: true);
            Screen.SetWireFrame(wireframe: false);

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();
            ui.SetExitAction(() => client.Close());
        }

        public void Update(float deltaTime)
        {
            // Method intentionally left empty.
        }

        public void OnResize(Vector2i size)
        {
            ui.Resize(size);
        }

        public void Render(float deltaTime)
        {
            ui.Render();
        }

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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~StartScene()
        {
            Dispose(disposing: false);
        }

        #endregion IDisposable Support
    }
}