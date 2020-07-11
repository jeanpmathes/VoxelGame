// <copyright file="Texture.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = OpenToolkit.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Rendering
{
    public class Texture : IDisposable
    {
        private static readonly ILogger logger = Program.CreateLogger<Texture>();

        public int Handle { get; }
        public TextureUnit TextureUnit { get; private set; }

        public Texture(string path, int fallbackResolution = 16)
        {
            Handle = GL.GenTexture();

            Use();

            try
            {
                using Bitmap bitmap = new Bitmap(path);
                SetupTexture(bitmap);
            }
            catch (Exception exception) when (exception is FileNotFoundException || exception is ArgumentException)
            {
                using (Bitmap bitmap = CreateFallback(fallbackResolution))
                {
                    SetupTexture(bitmap);
                }

                logger.LogWarning(LoggingEvents.MissingRessource, exception, "The texture could not be loaded and a fallback was used instead because the file was not found: {path}", path);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        private void SetupTexture(Bitmap bitmap)
        {
            Use();

            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                bitmap.Width,
                bitmap.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);
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
                    logger.LogWarning(LoggingEvents.UndeletedTexture, "A texture has been disposed by GC, without deleting the texture storage.");
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