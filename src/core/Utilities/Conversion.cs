// <copyright file="Conversion.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System.Drawing;

namespace VoxelGame.Core.Utilities
{
    public static class Conversion
    {
        /// <summary>
        /// Converts a <see cref="Color"/> to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The vector.</returns>
        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        }

        /// <summary>
        /// Converts a <see cref="Size"/> to a <see cref="Vector2i"/>.
        /// </summary>
        /// <param name="size">The size to convert.</param>
        /// <returns>The vector.</returns>
        public static Vector2i ToVector2i(this Size size)
        {
            return new Vector2i(size.Width, size.Height);
        }
    }
}