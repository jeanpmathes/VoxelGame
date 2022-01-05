// <copyright file="Texture.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Logging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace VoxelGame.Graphics.Objects
{
    /// <summary>
    ///     A texture.
    /// </summary>
    public class Texture : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Texture>();

        public Texture(string path, TextureUnit unit, int fallbackResolution = 16)
        {
            TextureUnit = unit;

            GL.CreateTextures(TextureTarget.Texture2D, n: 1, out int handle);
            Handle = handle;

            Use(TextureUnit);

            try
            {
                using var bitmap = new Bitmap(path);
                SetupTexture(bitmap);
            }
            catch (Exception exception) when (exception is FileNotFoundException or ArgumentException)
            {
                using (Bitmap bitmap = CreateFallback(fallbackResolution))
                {
                    SetupTexture(bitmap);
                }

                logger.LogWarning(
                    Events.MissingResource,
                    exception,
                    "The texture could not be loaded and a fallback was used instead because the file was not found: {Path}",
                    path);
            }

            GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

            GL.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

            GL.GenerateTextureMipmap(Handle);
        }

        private int Handle { get; }

        public TextureUnit TextureUnit { get; private set; }

        private void SetupTexture(Bitmap bitmap)
        {
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

            BitmapData data = bitmap.LockBits(
                new Rectangle(x: 0, y: 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            GL.TextureStorage2D(Handle, levels: 1, SizedInternalFormat.Rgba8, bitmap.Width, bitmap.Height);

            GL.TextureSubImage2D(
                Handle,
                level: 0,
                xoffset: 0,
                yoffset: 0,
                bitmap.Width,
                bitmap.Height,
                OpenToolkit.Graphics.OpenGL4.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.BindTextureUnit(unit - TextureUnit.Texture0, Handle);
            TextureUnit = unit;
        }

        public static Bitmap CreateFallback(int resolution)
        {
            var fallback = new Bitmap(resolution, resolution, PixelFormat.Format32bppArgb);

            Color magenta = Color.FromArgb(alpha: 64, red: 255, green: 0, blue: 255);
            Color black = Color.FromArgb(alpha: 64, red: 0, green: 0, blue: 0);

            for (var x = 0; x < fallback.Width; x++)
            for (var y = 0; y < fallback.Height; y++)
                if ((x % 2 == 0) ^ (y % 2 == 0)) fallback.SetPixel(x, y, magenta);
                else fallback.SetPixel(x, y, black);

            return fallback;
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) GL.DeleteTexture(Handle);
                else
                    logger.LogWarning(
                        Events.UndeletedTexture,
                        "Texture disposed by GC without freeing storage");

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