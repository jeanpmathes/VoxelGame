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

        public override int Count { get; }

        private readonly int arrayCount;
        private readonly TextureUnit[] textureUnits;
        private readonly int[] handles;

        private readonly Dictionary<string, int> textureIndicies;

        public ArrayTexture(string path, int resolution, bool useCustomMipmapGeneration, params TextureUnit[] textureUnits)
        {
            if (resolution <= 0 || (resolution & (resolution - 1)) != 0)
            {
                throw new ArgumentException($"The resolution '{resolution}' is either negative or not a power of two, which is not allowed.");
            }

            arrayCount = textureUnits.Length;

            this.textureUnits = textureUnits;
            handles = new int[arrayCount];

            GL.CreateTextures(TextureTarget.Texture2DArray, arrayCount, handles);

            string[] texturePaths;

            try
            {
                texturePaths = Directory.GetFiles(path, "*.png");
            }
            catch (DirectoryNotFoundException)
            {
                texturePaths = Array.Empty<string>();
                logger.LogWarning("A texture directory has not been found: {path}", path);
            }

            List<Bitmap> textures = new List<Bitmap>();
            textureIndicies = new Dictionary<string, int>();

            // Create fall back texture.
            Bitmap fallback = Texture.CreateFallback(resolution);
            textures.Add(fallback);

            // Split all images into separate bitmaps and create a list.
            LoadBitmaps(resolution, texturePaths, ref textures);

            // Check if the arrays could hold all textures
            if (textures.Count > 2048 * handles.Length)
            {
                logger.LogCritical(
                    "The number of textures found ({count}) is higher than the number of textures ({max}) that are allowed for an ArrayTexture using {units} units.",
                    textures.Count,
                    2048 * handles.Length,
                    textureUnits.Length);

                throw new ArgumentException("Too many textures in directory for this ArrayTexture!");
            }

            Count = textures.Count;

            int loadedTextures = 0;

            for (int i = 0; loadedTextures < textures.Count; i++)
            {
                int remainingTextures = textures.Count - loadedTextures;

                SetupArrayTexture(handles[i], textureUnits[i], resolution, textures, loadedTextures, loadedTextures + (remainingTextures < 2048 ? remainingTextures : 2048), useCustomMipmapGeneration);

                loadedTextures += 2048;
            }

            // Cleanup
            foreach (Bitmap bitmap in textures)
            {
                bitmap.Dispose();
            }

            logger.LogDebug("ArrayTexture with {count} textures loaded.", Count);
        }

        private void LoadBitmaps(int resolution, string[] paths, ref List<Bitmap> bitmaps)
        {
            if (paths.Length == 0) return;

            int texIndex = 1;

            for (int i = 0; i < paths.Length; i++)
            {
                try
                {
                    using Bitmap bitmap = new Bitmap(paths[i]);
                    if ((bitmap.Width % resolution) == 0 && bitmap.Height == resolution) // Check if image consists of correctly sized textures
                    {
                        int textureCount = bitmap.Width / resolution;
                        textureIndicies.Add(Path.GetFileNameWithoutExtension(paths[i]), texIndex);

                        for (int j = 0; j < textureCount; j++)
                        {
                            bitmaps.Add(bitmap.Clone(new Rectangle(j * resolution, 0, resolution, resolution), System.Drawing.Imaging.PixelFormat.Format32bppArgb));
                            texIndex++;
                        }
                    }
                    else
                    {
                        logger.LogDebug("The size of the image did not match the specified resolution ({resolution}) and was not loaded: {path}", resolution, paths[i]);
                    }
                }
                catch (FileNotFoundException e)
                {
                    logger.LogError(e, "The image could not be loaded: {path}", paths[i]);
                }
            }
        }

        private static void SetupArrayTexture(int handle, TextureUnit unit, int resolution, List<Bitmap> textures, int startIndex, int length, bool useCustomMipmapGeneration)
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
                for (int w = 0; w < lowerLevel.Width; w++)
                {
                    for (int h = 0; h < lowerLevel.Height; h++)
                    {
                        Color c1 = upperLevel.GetPixel(w * 2, h * 2);
                        Color c2 = upperLevel.GetPixel((w * 2) + 1, h * 2);
                        Color c3 = upperLevel.GetPixel(w * 2, (h * 2) + 1);
                        Color c4 = upperLevel.GetPixel((w * 2) + 1, (h * 2) + 1);

                        int minAlpha = Math.Min(Math.Min(c1.A, c2.A), Math.Min(c3.A, c4.A));
                        int maxAlpha = Math.Max(Math.Max(c1.A, c2.A), Math.Max(c3.A, c4.A));

                        int one = ((c1.A == 0) ? 0 : 1), two = ((c2.A == 0) ? 0 : 1), three = ((c3.A == 0) ? 0 : 1), four = ((c4.A == 0) ? 0 : 1);
                        double relevantPixels = (minAlpha != 0) ? 4 : one + two + three + four;

                        Color average = (relevantPixels == 0) ? Color.FromArgb(0, 0, 0, 0) :
                            Color.FromArgb(alpha: maxAlpha,
                                red: (int)Math.Sqrt(((c1.R * c1.R) + (c2.R * c2.R) + (c3.R * c3.R) + (c4.R * c4.R)) / relevantPixels),
                                green: (int)Math.Sqrt(((c1.G * c1.G) + (c2.G * c2.G) + (c3.G * c3.G) + (c4.G * c4.G)) / relevantPixels),
                                blue: (int)Math.Sqrt(((c1.B * c1.B) + (c2.B * c2.B) + (c3.B * c3.B) + (c4.B * c4.B)) / relevantPixels));

                        lowerLevel.SetPixel(w, h, average);
                    }
                }

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