// <copyright file="Texture.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenToolkit.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Rendering
{
    public class Texture : IDisposable
    {
        public int Handle { get; }
        public TextureUnit TextureUnit { get; private set; }

        public Texture(string path)
        {
            Handle = GL.GenTexture();

            Use();

            using (Bitmap image = new Bitmap(path))
            {
                image.RotateFlip(RotateFlipType.Rotate180FlipX);

                BitmapData data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            TextureUnit = unit;
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GL.DeleteTexture(Handle);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                    Console.WriteLine("WARNING: A texture has been disposed by GC, without deleting the texture storage.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                    Console.ResetColor();
                }

                disposed = true;
            }
        }

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
    }
}