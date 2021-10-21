// <copyright file="UserInterface.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.IO;
using Gwen.Net.Control;
using Gwen.Net.OpenTk;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Input;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.UserInterfaces
{
    public abstract class UserInterface : IDisposable
    {
        private readonly bool drawBackground;
        private readonly IGwenGui gui;
        private readonly InputListener inputListener;

        protected UserInterface(GameWindow window, InputListener inputListener, bool drawBackground)
        {
            gui = GwenGuiFactory.CreateFromGame(
                window,
                GwenGuiSettings.Default.From(
                    settings =>
                    {
                        settings.SkinFile = new FileInfo("DefaultSkin2.png");
                        settings.DrawBackground = drawBackground;
                    }));

            this.drawBackground = drawBackground;
            this.inputListener = inputListener;
        }

        internal Context Context { get; private set; } = null!;

        public ControlBase Root => gui.Root;

        public void Load()
        {
            gui.Load();
            gui.Root.ShouldDrawBackground = drawBackground;

            Context = new Context(new FontHolder(gui.Root.Skin), inputListener);
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
                if (disposing) gui.Dispose();

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