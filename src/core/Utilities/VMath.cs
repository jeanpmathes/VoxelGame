// <copyright file="VMath.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Core.Utilities
{
    /// <summary>
    /// A class containing different mathematical methods.
    /// </summary>
    public static class VMath
    {
        /// <summary>
        /// Clamps a vector between a minimum and maximum length.
        /// </summary>
        /// <param name="vector">The vector to clamp.</param>
        /// <param name="min">The minimum length.</param>
        /// <param name="max">The maximum length.</param>
        /// <returns>The clamped vector.</returns>
        public static Vector3 Clamp(Vector3 vector, float min, float max)
        {
            float length = vector.Length;

            if (length < min)
            {
                return vector.Normalized() * min;
            }
            else if (length > max)
            {
                return vector.Normalized() * max;
            }

            return vector;
        }

        /// <summary>
        /// Returns a copy of the vector where every component is positive.
        /// </summary>
        /// <param name="vector">The vector of which an absolute vector should be created.</param>
        /// <returns>The absolute vector.</returns>
        public static Vector3 Absolute(this Vector3 vector)
        {
            return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
        }

        /// <summary>
        /// Rounds every component of a vector.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns>The rounded vector.</returns>
        public static Vector3 Rounded(this Vector3 vector, int digits = 0)
        {
            return new Vector3((float)Math.Round(vector.X, digits), (float)Math.Round(vector.Y, digits), (float)Math.Round(vector.Z, digits));
        }

        /// <summary>
        /// Clamps every component of a vector.
        /// </summary>
        /// <param name="vector">The vector to clamp.</param>
        /// <param name="min">The minimum values for each component.</param>
        /// <param name="max">The maximum values for each component.</param>
        /// <returns>The vector with clamped components</returns>
        public static Vector3 ClampComponents(Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(MathHelper.Clamp(vector.X, min.X, max.X), MathHelper.Clamp(vector.Y, min.Y, max.Y), MathHelper.Clamp(vector.Z, min.Z, max.Z));
        }

        /// <summary>
        /// Returns a vector where every component is the sign of the original component.
        /// </summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The sign vector</returns>
        public static Vector3 Sign(this Vector3 vector)
        {
            return new Vector3(Math.Sign(vector.X), Math.Sign(vector.Y), Math.Sign(vector.Z));
        }

        /// <summary>
        /// Returns a vector where every component is the modulo of mod.
        /// </summary>
        /// <param name="vector">The vector to use.</param>
        /// <param name="mod">The number dividing.</param>
        /// <returns>The modulo vector.</returns>
        public static Vector3i Mod(this Vector3i vector, int mod)
        {
            return new Vector3i((vector.X % mod + mod) % mod, (vector.Y % mod + mod) % mod, (vector.Z % mod + mod) % mod);
        }
    }
}