// <copyright file="TextureAtlas.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = OpenToolkit.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Rendering
{
    public class ArrayTexture
    {
        public int Count { get; }

        public int HandleA { get; }
        public int HandleB { get; }

        private readonly TextureUnit unitA;
        private readonly TextureUnit unitB;

        private readonly Dictionary<string, int> textureIndicies;

        public ArrayTexture(string path, int resolution, bool useCustomMipmapGeneration, TextureUnit unitA, TextureUnit unitB)
        {
            if (resolution <= 0 || (resolution & (resolution - 1)) != 0)
            {
                throw new ArgumentException($"The resolution '{resolution}' is either negative or not a power of two, which is not allowed.");
            }

            this.unitA = unitA;
            this.unitB = unitB;

            HandleA = GL.GenTexture();
            HandleB = GL.GenTexture();

            string[] texturePaths = Directory.GetFiles(path, "*.png");
            List<Bitmap> textures = new List<Bitmap>();
            textureIndicies = new Dictionary<string, int>();

            int currentIndex = 0;

            for (int i = 0; i < texturePaths.Length; i++) // Split all images into separate bitmaps and create a list
            {
                try
                {
                    using Bitmap bitmap = new Bitmap(texturePaths[i]);
                    if ((bitmap.Width % resolution) == 0 && bitmap.Height == resolution) // Check if image consists of correctly sized textures
                    {
                        int textureCount = bitmap.Width / resolution;
                        textureIndicies.Add(Path.GetFileNameWithoutExtension(texturePaths[i]), currentIndex);

                        for (int j = 0; j < textureCount; j++)
                        {
                            textures.Add(bitmap.Clone(new Rectangle(j * resolution, 0, resolution, resolution), System.Drawing.Imaging.PixelFormat.Format32bppArgb));
                            currentIndex++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"The image has the wrong width or height: {texturePaths[i]}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"The image could not be loaded: {texturePaths[i]}");
                    Console.WriteLine(e);

                    throw;
                }
            }

            // Check if the arrays could hold all textures
            if (textures.Count > 4096)
            {
                throw new ArgumentException($"More than 4096 ({textures.Count}) textures were found; only 4096 textures can be stored in one {nameof(ArrayTexture)}!");
            }

            Count = textures.Count;

            int countA, countB;
            if (textures.Count > 2048)
            {
                countA = 2048;
                countB = textures.Count - 2048;
            }
            else
            {
                countA = textures.Count;
                countB = 0;
            }

            SetupArrayTexture(HandleA, unitA, resolution, textures, 0, countA, useCustomMipmapGeneration);
            if (countB != 0) SetupArrayTexture(HandleB, unitB, resolution, textures, 2049, countB, useCustomMipmapGeneration);

            // Cleanup
            foreach (Bitmap bitmap in textures)
            {
                bitmap.Dispose();
            }
        }

        private static void SetupArrayTexture(int handle, TextureUnit unit, int resolution, List<Bitmap> textures, int startIndex, int length, bool useCustomMipmapGeneration)
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
                GenerateMipmapWithoutTransparencyMixing(container, levels, length);
            }

            // Set texture parameters for array
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        private static void GenerateMipmapWithoutTransparencyMixing(Bitmap baseLevel, int levels, int length)
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
                        int relevantPixels = (minAlpha != 0) ? 4 : ((c1.A == 0) ? 0 : 1) + ((c2.A == 0) ? 0 : 1) + ((c3.A == 0) ? 0 : 1) + ((c4.A == 0) ? 0 : 1);

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
                GL.TexSubImage3D(TextureTarget.Texture2DArray, lod, 0, 0, 0, lowerLevel.Width, lowerLevel.Width, length, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

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

        public void Use()
        {
            // Bind A
            GL.ActiveTexture(unitA);
            GL.BindTexture(TextureTarget.Texture2DArray, HandleA);

            // Bind B
            GL.ActiveTexture(unitB);
            GL.BindTexture(TextureTarget.Texture2DArray, HandleB);
        }

        public int GetTextureIndex(string name)
        {
            if (textureIndicies.TryGetValue(name, out int value))
            {
                return value;
            }
            else
            {
                throw new ArgumentException($"There is no texture with the name: {name}");
            }
        }
    }
}