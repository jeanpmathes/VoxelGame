// <copyright file="GameUI.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Gwen.Net.OpenTk;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using System;

namespace VoxelGame.UI
{
    public class GameUI : IDisposable
    {
        private readonly IGwenGui gui;
        private bool disposedValue;

        private GameControl control = null!;

        public GameUI(GameWindow window)
        {
            gui = GwenGuiFactory.CreateFromGame(window, GwenGuiSettings.Default.From((settings) =>
            {
                settings.SkinFile = new System.IO.FileInfo("DefaultSkin2.png");
                settings.DrawBackground = false;
            }));
        }

        public void Load()
        {
            gui.Load();
            gui.Root.ShouldDrawBackground = false;
            control = new GameControl(gui.Root);
        }

        public void Render()
        {
            GL.Disable(EnableCap.CullFace);

            gui.Render();

            GL.Enable(EnableCap.CullFace);
        }

        public void Resize(Vector2i size)
        {
            gui.Resize(size);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    gui.Dispose();
                    control.Dispose();
                }

                disposedValue = true;
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