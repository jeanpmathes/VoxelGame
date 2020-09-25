// <copyright file="TextureAtlas.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Drawing;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Rendering
{
    public abstract class ArrayTexture : IDisposable, ITextureIndexProvider
    {
        public abstract int Count { get; }

        public abstract void Use();

        internal abstract void SetWrapMode(TextureWrapMode mode);

        public abstract int GetTextureIndex(string name);

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