// <copyright file="UserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net.Control;
using Gwen.Net.OpenTk;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using System;

namespace VoxelGame.UI
{
    public abstract class UserInterface : IDisposable
    {
        public ControlBase Root { get => gui.Root; }

        private readonly IGwenGui gui;
        private readonly bool drawBackground;

        protected UserInterface(GameWindow window, bool drawBackground)
        {
            gui = GwenGuiFactory.CreateFromGame(window, GwenGuiSettings.Default.From((settings) =>
            {
                settings.SkinFile = new System.IO.FileInfo("DefaultSkin2.png");
                settings.DrawBackground = drawBackground;
            }));

            this.drawBackground = drawBackground;
        }

        public void Load()
        {
            gui.Load();
            gui.Root.ShouldDrawBackground = drawBackground;
            gui.Root.Skin.DefaultFont.Size = 15;
        }

        public abstract void CreateControl();

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

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    gui.Dispose();
                }

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