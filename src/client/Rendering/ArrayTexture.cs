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
using System.IO;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Rendering
{
    public abstract class ArrayTexture : IDisposable, ITextureIndexProvider
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ArrayTexture>();

        private protected readonly Dictionary<string, int> textureIndicies = new Dictionary<string, int>();

        public abstract int Count { get; protected set; }

        public abstract void Use();

        internal abstract void SetWrapMode(TextureWrapMode mode);

        private protected int arrayCount;
        private protected TextureUnit[] textureUnits = null!;
        private protected int[] handles = null!;

        protected void Initialize(string path, int resolution, bool useCustomMipmapGeneration, params TextureUnit[] textureUnits)
        {
            if (resolution <= 0 || (resolution & (resolution - 1)) != 0)
            {
                throw new ArgumentException($"The resolution '{resolution}' is either negative or not a power of two, which is not allowed.");
            }

            arrayCount = textureUnits.Length;

            this.textureUnits = textureUnits;
            handles = new int[arrayCount];

            GetHandles(handles);

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

        protected abstract void GetHandles(int[] arr);

        protected abstract void SetupArrayTexture(int handle, TextureUnit unit, int resolution, List<Bitmap> textures, int startIndex, int length, bool useCustomMipmapGeneration);

        /// <summary>
        /// Loads all bitmaps specified by the paths into the list. The bitmaps are split into smaller parts that are all sized according to the resolution.
        /// </summary>
        /// <remarks>
        /// Textures provided have to have the height given by the resolution, and the width must be a multiple of it.
        /// </remarks>
        protected void LoadBitmaps(int resolution, string[] paths, ref List<Bitmap> bitmaps)
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

        protected void GenerateMipmapWithoutTransparencyMixing(int handle, Bitmap baseLevel, int levels, int length)
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
                UploadPixelData(handle, lowerLevel, lod, length);

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

        protected abstract void UploadPixelData(int handle, Bitmap bitmap, int lod, int length);

        /// <summary>
        /// Method used in generating a custom mipmap.
        /// </summary>
        /// <param name="upperLevel"></param>
        /// <param name="lowerLevel"></param>
        protected static void CreateLowerLevel(ref Bitmap upperLevel, ref Bitmap lowerLevel)
        {
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
        }

        public int GetTextureIndex(string name)
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

        protected abstract void Dispose(bool disposing);

        ~ArrayTexture()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposalbe Support
    }
}