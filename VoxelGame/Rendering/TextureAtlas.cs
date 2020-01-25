using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace VoxelGame.Rendering
{
    public class TextureAtlas
    {
        public readonly int Handle;
        public readonly int extents;

        private readonly Dictionary<string, int> textureIndicies;
        private readonly int log2Extents;

        public TextureAtlas(string path)
        {
            Handle = GL.GenTexture();

            Use();

            string[] texturePaths = Directory.GetFiles(path, "*.png");
            List<Bitmap> textures = new List<Bitmap>();
            textureIndicies = new Dictionary<string, int>();

            int currentIndex = 0;

            for (int i = 0; i < texturePaths.Length; i++) // Split all images into separate bitmpas and create a list
            {
                try
                {
                    using (Bitmap bitmap = new Bitmap(texturePaths[i]))
                    {
                        if ((bitmap.Width & 15) == 0 && bitmap.Height == 16) // Check if image consists of 16x16 textures
                        {
                            int textureCount = bitmap.Width >> 4;
                            textureIndicies.Add(Path.GetFileNameWithoutExtension(texturePaths[i]), currentIndex);

                            for (int j = 0; j < textureCount; j++)
                            {
                                textures.Add(bitmap.Clone(new Rectangle(j << 4, 0, 16, 16), System.Drawing.Imaging.PixelFormat.Format32bppArgb));
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
                }
            }

            extents = (int)Math.Sqrt(Math.Pow(2, (Math.Floor(Math.Log(currentIndex, 2)) + 1))); // Calculate the extents of the atlas
            log2Extents = (int)Math.Log(extents, 2);

            // Create a single bitmap from all textures
            using (Bitmap atlas = new Bitmap(extents * 16, extents * 16))
            {
                using (Graphics canvas = Graphics.FromImage(atlas))
                {
                    for (int i = 0; i < textures.Count; i++)
                    {
                        canvas.DrawImage(textures[i], (i & (extents - 1)) * 16, (i >> log2Extents) * 16, textures[i].Width, textures[i].Height);
                    }

                    canvas.Save();
                }

                atlas.RotateFlip(RotateFlipType.Rotate180FlipNone);

                BitmapData data = atlas.LockBits(
                    new Rectangle(0, 0, atlas.Width, atlas.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    atlas.Width,
                    atlas.Height,
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

            // Cleanup
            foreach (Bitmap bitmap in textures)
            {
                bitmap.Dispose();
            }
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public int GetTextureIndex(string name)
        {
            if (textureIndicies.TryGetValue(name, out int value))
            {
                return value;
            }
            else
            {
                return -1;
            }
        }

        public AtlasPosition GetTextureUV(int index)
        {
            return new AtlasPosition(1f - 1f / extents * ((index & (extents - 1)) + 1f), 1f - 1f / extents * ((index >> log2Extents) + 1f), 1f - 1f / extents * (index & (extents - 1)), 1f - 1f / extents * (index >> log2Extents));
        }
    }
}
