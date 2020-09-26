// <copyright file="Texture.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Drawing;

namespace VoxelGame.Client.Rendering
{
    public abstract class Texture : IDisposable
    {
        public abstract int Handle { get; }
        public TextureUnit TextureUnit { get; protected set; }

        public abstract void Use(TextureUnit unit = TextureUnit.Texture0);

        #region IDisposable Support

        protected abstract void Dispose(bool disposing);

        ~Texture()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        internal static Bitmap CreateFallback(int resolution)
        {
            Bitmap fallback = new Bitmap(resolution, resolution, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Color magenta = Color.FromArgb(64, 255, 0, 255);
            Color black = Color.FromArgb(64, 0, 0, 0);

            for (int x = 0; x < fallback.Width; x++)
            {
                for (int y = 0; y < fallback.Height; y++)
                {
                    if (x % 2 == 0 ^ y % 2 == 0)
                    {
                        fallback.SetPixel(x, y, magenta);
                    }
                    else
                    {
                        fallback.SetPixel(x, y, black);
                    }
                }
            }

            return fallback;
        }
    }
}