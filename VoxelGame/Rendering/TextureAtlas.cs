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
    public class TextureAtlas : IDisposable
    {
        private readonly int extents;

        private readonly Dictionary<string, int> textureIndicies;
        private readonly int log2Extents;

        public int Handle { get; }

        public TextureAtlas(string path)
        {
            Handle = GL.GenTexture();

            Use();

            string[] texturePaths = Directory.GetFiles(path, "*.png");
            List<Bitmap> textures = new List<Bitmap>();
            textureIndicies = new Dictionary<string, int>();

            int currentIndex = 0;

            for (int i = 0; i < texturePaths.Length; i++) // Split all images into separate bitmaps and create a list
            {
                try
                {
                    using Bitmap bitmap = new Bitmap(texturePaths[i]);

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
                catch (Exception e)
                {
                    Console.WriteLine($"The image could not be loaded: {texturePaths[i]}");
                    Console.WriteLine(e);

                    throw;
                }
            }

            // Calculate the extents of the atlas
            extents = (int)Math.Ceiling(Math.Sqrt(textures.Count));

            extents--;
            extents |= extents >> 1;
            extents |= extents >> 2;
            extents |= extents >> 4;
            extents |= extents >> 8;
            extents |= extents >> 16;
            extents++;

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
                throw new ArgumentException($"There is no texture with the name: {name}");
            }
        }

        public AtlasPosition GetTextureUV(int index)
        {
            return new AtlasPosition(1f - (1f / extents * ((index & (extents - 1)) + 1f)), 1f - (1f / extents * ((index >> log2Extents) + 1f)), 1f - (1f / extents * (index & (extents - 1))), 1f - (1f / extents * (index >> log2Extents)));
        }

        #region IDisposalbe Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GL.DeleteTexture(Handle);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                Console.WriteLine("WARNING: A texture has been disposed by GC, without deleting the texture storage.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                Console.ResetColor();

                disposed = true;
            }
        }

        ~TextureAtlas()
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

    /// <summary>
    /// The position of a texture in a texture atlas.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not Used")]
    public struct AtlasPosition
    {
        public float BottomLeftU { get; }
        public float BottomLeftV { get; }
        public float TopRightU { get; }
        public float TopRightV { get; }

        public AtlasPosition(float bottomLeftU, float bottomLeftV, float topRightU, float topRightV)
        {
            this.BottomLeftU = bottomLeftU;
            this.BottomLeftV = bottomLeftV;
            this.TopRightU = topRightU;
            this.TopRightV = topRightV;
        }
    }
}