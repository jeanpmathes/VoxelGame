// <copyright file="TintColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;

namespace VoxelGame.Core.Visuals
{
    /// <summary>
    /// A tint that can be applied to blocks.
    /// </summary>
    public readonly struct TintColor : IEquatable<TintColor>
    {
        private readonly float r;
        private readonly float g;
        private readonly float b;

        public bool IsNeutral { get; }

        public TintColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;

            IsNeutral = false;
        }

        public TintColor(float r, float g, float b, bool isNeutral)
        {
            this.r = r;
            this.g = g;
            this.b = b;

            IsNeutral = isNeutral;
        }

        public static TintColor None { get => new TintColor(1f, 1f, 1f); }
        public static TintColor Neutral { get => new TintColor(0f, 0f, 0f, true); }

        #region PREDEFINED COLORS

        /// <summary>
        /// Gets a white color: <c>(1|1|1)</c>
        /// </summary>
        public static TintColor White { get => new TintColor(1f, 1f, 1f); }

        /// <summary>
        /// Gets a red color: <c>(1|0|0)</c>
        /// </summary>
        public static TintColor Red { get => new TintColor(1f, 0f, 0f); }

        /// <summary>
        /// Gets a green color: <c>(0|1|0)</c>
        /// </summary>
        public static TintColor Green { get => new TintColor(0f, 1f, 0f); }

        /// <summary>
        /// Gets a blue color: <c>(0|0|1)</c>
        /// </summary>
        public static TintColor Blue { get => new TintColor(0f, 0f, 1f); }

        /// <summary>
        /// Gets a yellow color: <c>(1|1|0)</c>
        /// </summary>
        public static TintColor Yellow { get => new TintColor(1f, 1f, 0f); }

        /// <summary>
        /// Gets a cyan color: <c>(0|1|1)</c>
        /// </summary>
        public static TintColor Cyan { get => new TintColor(0f, 1f, 1f); }

        /// <summary>
        /// Gets a magenta color: <c>(1|0|1)</c>
        /// </summary>
        public static TintColor Magenta { get => new TintColor(1f, 0f, 1f); }

        /// <summary>
        /// Gets an orange color: <c>(1|0.5|0)</c>
        /// </summary>
        public static TintColor Orange { get => new TintColor(1f, 0.5f, 0f); }

        /// <summary>
        /// Gets a dark green color: <c>(0|0.5|0)</c>
        /// </summary>
        public static TintColor DarkGreen { get => new TintColor(0f, 0.5f, 0f); }

        /// <summary>
        /// Gets a lime color: <c>(0.75|1|0)</c>
        /// </summary>
        public static TintColor Lime { get => new TintColor(0.75f, 1f, 0f); }

        /// <summary>
        /// Gets a gray color: <c>(0.15|0.15|0.15)</c>
        /// </summary>
        public static TintColor Gray { get => new TintColor(0.15f, 0.15f, 0.15f); }

        /// <summary>
        /// Gets a light color: <c>(0.8|0.8|0.8)</c>
        /// </summary>
        public static TintColor LightGray { get => new TintColor(0.8f, 0.8f, 0.8f); }

        /// <summary>
        /// Gets an indigo color: <c>(0.5|1|0)</c>
        /// </summary>
        public static TintColor Indigo { get => new TintColor(0.5f, 1f, 0f); }

        /// <summary>
        /// Gets a maroon color: <c>(0.5|0|0)</c>
        /// </summary>
        public static TintColor Maroon { get => new TintColor(0.5f, 0f, 0f); }

        /// <summary>
        /// Gets an olive color: <c>(0.5|0.5|0)</c>
        /// </summary>
        public static TintColor Olive { get => new TintColor(0.5f, 0.5f, 0f); }

        /// <summary>
        /// Gets a brown color: <c>(0.5|0.25|0)</c>
        /// </summary>
        public static TintColor Brown { get => new TintColor(0.5f, 0.25f, 0f); }

        /// <summary>
        /// Gets a navy color: <c>(0|0|0.5)</c>
        /// </summary>
        public static TintColor Navy { get => new TintColor(0f, 0f, 0.5f); }

        /// <summary>
        /// Gets an amaranth color: <c>(0.9|0.2|0.3)</c>
        /// </summary>
        public static TintColor Amaranth { get => new TintColor(0.9f, 0.2f, 0.3f); }

        /// <summary>
        /// Gets an amber color: <c>(1|0.75|0)</c>
        /// </summary>
        public static TintColor Amber { get => new TintColor(1f, 0.75f, 0f); }

        /// <summary>
        /// Gets an apricot color: <c>(1|0.8|0.65)</c>
        /// </summary>
        public static TintColor Apricot { get => new TintColor(1f, 0.8f, 0.65f); }

        /// <summary>
        /// Gets an aquamarine color: <c>(0.5|1|0.85)</c>
        /// </summary>
        public static TintColor Aquamarine { get => new TintColor(0.5f, 1f, 0.85f); }

        /// <summary>
        /// Gets a beige color: <c>(0.9|0.9|0.8)</c>
        /// </summary>
        public static TintColor Beige { get => new TintColor(0.9f, 0.9f, 0.8f); }

        /// <summary>
        /// Gets a coffee color: <c>(0.45|0.3|0.2)</c>
        /// </summary>
        public static TintColor Coffee { get => new TintColor(0.45f, 0.3f, 0.2f); }

        /// <summary>
        /// Gets a coral color: <c>(1|0.5|0.3)</c>
        /// </summary>
        public static TintColor Coral { get => new TintColor(1f, 0.5f, 0.3f); }

        /// <summary>
        /// Gets a crimson color: <c>(0.9|0.15|0.3)</c>
        /// </summary>
        public static TintColor Crimson { get => new TintColor(0.9f, 0.15f, 0.3f); }

        /// <summary>
        /// Gets an emerald color: <c>(0.3|0.8|0.5)</c>
        /// </summary>
        public static TintColor Emerald { get => new TintColor(0.3f, 0.8f, 0.5f); }

        /// <summary>
        /// Gets a lilac color: <c>(0.8|0.6|0.8)</c>
        /// </summary>
        public static TintColor Lilac { get => new TintColor(0.8f, 0.6f, 0.8f); }

        /// <summary>
        /// Gets a mauve color: <c>(0.9|0.7|1)</c>
        /// </summary>
        public static TintColor Mauve { get => new TintColor(0.9f, 0.7f, 1f); }

        /// <summary>
        /// Gets a periwinkle color: <c>(0.8|0.8|1)</c>
        /// </summary>
        public static TintColor Periwinkle { get => new TintColor(0.8f, 0.8f, 1f); }

        /// <summary>
        /// Gets a Prussian blue color: <c>(0|0.2|0.32)</c>
        /// </summary>
        public static TintColor PrussianBlue { get => new TintColor(0f, 0.2f, 0.32f); }

        /// <summary>
        /// Gets a slate gray color: <c>(0.5|0.5|0.6)</c>
        /// </summary>
        public static TintColor SlateGray { get => new TintColor(0.5f, 0.5f, 0.6f); }

        /// <summary>
        /// Gets a taupe color: <c>(0.3|0.2|0.2)</c>
        /// </summary>
        public static TintColor Taupe { get => new TintColor(0.3f, 0.2f, 0.2f); }

        /// <summary>
        /// Gets a viridian color: <c>(0.3|0.5|0.45)</c>
        /// </summary>
        public static TintColor Viridian { get => new TintColor(0.3f, 0.5f, 0.45f); }

        #endregion PREDEFINED COLORS

        public int ToBits
        {
            get
            {
                return ((int) (r * 7f) << 6) | ((int) (g * 7f) << 3) | (int) (b * 7f);
            }
        }

        public int GetBits(TintColor neutral)
        {
            return IsNeutral ? neutral.ToBits : this.ToBits;
        }

        public override bool Equals(object? obj)
        {
            if (obj is TintColor other)
            {
                return (other.r == r) && (other.g == g) && (other.b == b) && (other.IsNeutral == IsNeutral);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(TintColor other)
        {
            return (other.r == r) && (other.g == g) && (other.b == b) && (other.IsNeutral == IsNeutral);
        }

        public override int GetHashCode()
        {
            return ((IsNeutral) ? 1 : 0 << 9) | ((int) (r * 7f) << 6) | ((int) (g * 7f) << 3) | (int) (b * 7f);
        }

        public static bool operator ==(TintColor left, TintColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TintColor left, TintColor right)
        {
            return !(left == right);
        }
    }
}