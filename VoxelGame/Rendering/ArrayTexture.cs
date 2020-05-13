// <copyright file="TextureAtlas.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Rendering
{
    public class ArrayTexture
    {
        public int HandleA { get; }
        public int HandleB { get; }

        private readonly TextureUnit unitA;
        private readonly TextureUnit unitB;

        private readonly Dictionary<string, int> textureIndicies;

        public ArrayTexture(string path, int resolution, TextureUnit unitA, TextureUnit unitB)
        {
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
                    using (Bitmap bitmap = new Bitmap(texturePaths[i]))
                    {
                        if ((bitmap.Width % resolution) == 0 && bitmap.Height == resolution) // Check if image consists of correctly sized textures
                        {
                            int textureCount = bitmap.Width >> 4;
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

            SetupArrayTexture(HandleA, unitA, resolution, textures, 0, countA);
            if (countB != 0) SetupArrayTexture(HandleB, unitB, resolution, textures, 2049, countB);

            // Cleanup
            foreach (Bitmap bitmap in textures)
            {
                bitmap.Dispose();
            }
        }

        private void SetupArrayTexture(int handle, TextureUnit unit, int resolution, List<Bitmap> textures, int startIndex, int length)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2DArray, handle);

            // Allocate storage for array
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, (int)Math.Log(resolution, 2), SizedInternalFormat.Rgba8, resolution, resolution, length);

            // Upload pixel data to array
            using (Bitmap container = new Bitmap(resolution, resolution * length))
            {
                using (Graphics canvas = Graphics.FromImage(container))
                {
                    for (int i = startIndex; i < length; i++)
                    {
                        textures[i].RotateFlip(RotateFlipType.RotateNoneFlipY);
                        canvas.DrawImage(textures[i], 0, i * resolution, resolution, resolution);
                    }

                    canvas.Save();
                }

                BitmapData data = container.LockBits(new Rectangle(0, 0, container.Width, container.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, resolution, resolution, length, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            }

            // Set texture parameters for array
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Generate mipmaps for array
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
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