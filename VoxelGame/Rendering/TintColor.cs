// <copyright file="TintColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// A tint that can be applied to blocks.
    /// </summary>
    public struct TintColor : IEquatable<TintColor>
    {
        private readonly float r;
        private readonly float g;
        private readonly float b;

        public bool IsNeutral { get; set; }

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

        public int ToBits
        {
            get
            {
                return ((int)(r * 7f) << 6) | ((int)(g * 7f) << 3) | (int)(b * 7f);
            }
        }

        public override bool Equals(object obj)
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
            return ((IsNeutral) ? 1 : 0 << 9) | ((int)(r * 7f) << 6) | ((int)(g * 7f) << 3) | (int)(b * 7f);
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
