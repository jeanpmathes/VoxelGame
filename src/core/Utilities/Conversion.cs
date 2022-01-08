// <copyright file="Conversion.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Utilities
{
    /// <summary>
    ///     A utility class for different conversion methods.
    /// </summary>
    public static class Conversion
    {
        /// <summary>
        ///     Converts a <see cref="Color" /> to a <see cref="Vector3" />.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The vector.</returns>
        public static Vector3 ToVector3(this Color color)
        {
            return new(color.R / 255f, color.G / 255f, color.B / 255f);
        }

        /// <summary>
        ///     Converts a <see cref="Size" /> to a <see cref="Vector2i" />.
        /// </summary>
        /// <param name="size">The size to convert.</param>
        /// <returns>The vector.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Vector2i ToVector2i(this Size size)
        {
            return new(size.Width, size.Height);
        }
    }
}