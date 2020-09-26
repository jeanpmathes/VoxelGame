﻿// <copyright file="TextureAtlas.cs" company="VoxelGame">
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
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using PixelFormat = OpenToolkit.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
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
            GL.GenTextures(arrayCount, arr);
        }

        protected override void SetupArrayTexture(int handle, TextureUnit unit, int resolution, List<Bitmap> textures, int startIndex, int length, bool useCustomMipmapGeneration)
        {
            int levels = (int)Math.Log(resolution, 2);

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2DArray, handle);

            // Allocate storage for array
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, levels, SizedInternalFormat.Rgba8, resolution, resolution, length);

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
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, resolution, resolution, length, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            container.UnlockBits(data);

            // Generate mipmaps for array
            if (!useCustomMipmapGeneration)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
            }
            else
            {
                GenerateMipmapWithoutTransparencyMixing(handle, container, levels, length);
            }

            // Set texture parameters for array
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        protected override void UploadPixelData(int handle, Bitmap bitmap, int lod, int length)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, lod, 0, 0, 0, bitmap.Width, bitmap.Width, length, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
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
                GL.ActiveTexture(textureUnits[i]);
                GL.BindTexture(TextureTarget.Texture2DArray, handles[i]);

                GL.TextureParameter(handles[i], TextureParameterName.TextureWrapS, (int)mode);
                GL.TextureParameter(handles[i], TextureParameterName.TextureWrapT, (int)mode);
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