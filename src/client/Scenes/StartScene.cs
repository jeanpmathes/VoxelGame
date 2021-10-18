// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public class StartScene : IScene
    {
        private readonly Application.Client client;
        private readonly StartUserInterface ui;

        private readonly WorldManager worldManager;

        internal StartScene(Application.Client client)
        {
            this.client = client;
            worldManager = new WorldManager(client.worldsDirectory);
            worldManager.WorldActivation += client.LoadGameScene;

            ui = new StartUserInterface(client, worldManager, drawBackground: true);
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

        #endregion IDisposable Support
    }
}