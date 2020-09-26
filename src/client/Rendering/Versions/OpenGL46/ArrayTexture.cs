// <copyright file="TextureAtlas.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using PixelFormat = OpenToolkit.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Client.Rendering.Versions.OpenGL46
{
    public class ArrayTexture : Rendering.ArrayTexture
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ArrayTexture>();

        public override int Count { get; protected set; }

        public ArrayTexture(string path, int resolution, bool useCustomMipmapGeneration, params TextureUnit[] textureUnits)
        {
            Initialize(path, resolution, useCustomMipmapGeneration, textureUnits);
        }

        protected override void GetHandles(int[] arr)
        {
            GL.CreateTextures(TextureTarget.Texture2DArray, arrayCount, handles);
        }

        protected override void SetupArrayTexture(int handle, TextureUnit unit, int resolution, List<Bitmap> textures, int startIndex, int length, bool useCustomMipmapGeneration)
        {
            int levels = (int)Math.Log(resolution, 2);

            GL.BindTextureUnit(unit - TextureUnit.Texture0, handle);

            // Allocate storage for array
            GL.TextureStorage3D(handle, levels, SizedInternalFormat.Rgba8, resolution, resolution, length);

            using Bitmap container = new Bitmap(resolution, resolution * length);
            using (Graphics canvas = Graphics.FromImage(container))
            {
                // Combine all textures into one
                for (int i = startIndex; i < length; i++)
                {
                    textures[i].RotateFlip(RotateFlipType.RotateNoneFlipY);
                    canvas.DrawImage(textures[i], 0, i * resolution, resolution, resolution);
                }

                canvas.Save();
            }

            // Upload pixel data to array
            BitmapData data = container.LockBits(new Rectangle(0, 0, container.Width, container.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TextureSubImage3D(handle, 0, 0, 0, 0, resolution, resolution, length, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            container.UnlockBits(data);

            // Generate mipmaps for array
            if (!useCustomMipmapGeneration)
            {
                GL.GenerateTextureMipmap(handle);
            }
            else
            {
                GenerateMipmapWithoutTransparencyMixing(handle, container, levels, length);
            }

            // Set texture parameters for array
            GL.TextureParameter(handle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TextureParameter(handle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TextureParameter(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TextureParameter(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        private static void GenerateMipmapWithoutTransparencyMixing(int handle, Bitmap baseLevel, int levels, int length)
        {
            Bitmap upperLevel = baseLevel;

            for (int lod = 1; lod < levels; lod++)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                Bitmap lowerLevel = new Bitmap(upperLevel.Width / 2, upperLevel.Height / 2);
#pragma warning restore CA2000 // Dispose objects before losing scope

                // Create the lower level by averaging the upper level
                CreateLowerLevel(ref upperLevel, ref lowerLevel);

                // Upload pixel data to array
                BitmapData data = lowerLevel.LockBits(new Rectangle(0, 0, lowerLevel.Width, lowerLevel.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TextureSubImage3D(handle, lod, 0, 0, 0, lowerLevel.Width, lowerLevel.Width, length, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                lowerLevel.UnlockBits(data);

                if (!upperLevel.Equals(baseLevel))
                {
                    upperLevel?.Dispose();
                }

                upperLevel = lowerLevel;
            }

            if (!upperLevel.Equals(baseLevel))
            {
                upperLevel?.Dispose();
            }
        }

        public override void Use()
        {
            for (int i = 0; i < arrayCount; i++)
            {
                GL.BindTextureUnit(textureUnits[i] - TextureUnit.Texture0, handles[i]);
            }
        }

        internal override void SetWrapMode(TextureWrapMode mode)
        {
            for (int i = 0; i < arrayCount; i++)
            {
                GL.BindTextureUnit(textureUnits[i] - TextureUnit.Texture0, handles[i]);

                GL.TextureParameter(handles[i], TextureParameterName.TextureWrapS, (int)mode);
                GL.TextureParameter(handles[i], TextureParameterName.TextureWrapT, (int)mode);
            }
        }

        public override int GetTextureIndex(string name)
        {
            if (name == "missing_texture")
            {
                return 0;
            }

            if (textureIndicies.TryGetValue(name, out int value))
            {
                return value;
            }
            else
            {
                logger.LogWarning(LoggingEvents.MissingRessource, "The texture '{name}' is not available, fallback is used.", name);

                return 0;
            }
        }

        #region IDisposalbe Support

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    for (int i = 0; i < arrayCount; i++)
                    {
                        GL.DeleteTexture(handles[i]);
                    }
                }

                logger.LogWarning(LoggingEvents.UndeletedTexture, "A texture has been disposed by GC, without deleting the texture storage.");

                disposed = true;
            }
        }

        #endregion IDisposalbe Support
    }
}