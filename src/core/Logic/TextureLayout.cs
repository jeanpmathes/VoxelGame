// <copyright file="TextureLayout.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    /// Provides functionality to define the textures of a default six-sided block or a liquid.
    /// </summary>
    public readonly struct TextureLayout : IEquatable<TextureLayout>
    {
        private static ITextureIndexProvider _blockTextureIndexProvider = null!;
        private static ITextureIndexProvider _liquidTextureIndexProvider = null!;

        public static void SetProviders(ITextureIndexProvider blockTextureIndexProvider,
            ITextureIndexProvider liquidTextureIndexProvider)
        {
            _blockTextureIndexProvider = blockTextureIndexProvider;
            _liquidTextureIndexProvider = liquidTextureIndexProvider;
        }

        public int Front { get; }
        public int Back { get; }
        public int Left { get; }
        public int Right { get; }
        public int Bottom { get; }
        public int Top { get; }

        public TextureLayout(int front, int back, int left, int right, int bottom, int top)
        {
            Front = front;
            Back = back;
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }

        /// <summary>
        /// Returns a texture layout where every side has the same texture.
        /// </summary>
        public static TextureLayout Uniform(string texture)
        {
            int i = _blockTextureIndexProvider.GetTextureIndex(texture);

            return new TextureLayout(i, i, i, i, i, i);
        }

        /// <summary>
        /// Returns a texture layout where every side has a different texture.
        /// </summary>
        public static TextureLayout Unique(string front, string back, string left, string right, string bottom,
            string top)
        {
            return new TextureLayout(
                front: _blockTextureIndexProvider.GetTextureIndex(front),
                back: _blockTextureIndexProvider.GetTextureIndex(back),
                left: _blockTextureIndexProvider.GetTextureIndex(left),
                right: _blockTextureIndexProvider.GetTextureIndex(right),
                bottom: _blockTextureIndexProvider.GetTextureIndex(bottom),
                top: _blockTextureIndexProvider.GetTextureIndex(top));
        }

        /// <summary>
        /// Returns a texture layout where two textures are used, one for top/bottom, the other for the sides around it.
        /// </summary>
        public static TextureLayout Column(string sides, string ends)
        {
            int sideIndex = _blockTextureIndexProvider.GetTextureIndex(sides);
            int endIndex = _blockTextureIndexProvider.GetTextureIndex(ends);

            return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, endIndex, endIndex);
        }

        /// <summary>
        /// Returns a texture layout where two textures are used, one for top/bottom, the other for the sides around it.
        /// </summary>
        public static TextureLayout Column(string texture, int sideOffset, int endOffset)
        {
            int sideIndex = _blockTextureIndexProvider.GetTextureIndex(texture) + sideOffset;
            int endIndex = _blockTextureIndexProvider.GetTextureIndex(texture) + endOffset;

            return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, endIndex, endIndex);
        }

        /// <summary>
        /// Returns a texture layout where three textures are used, one for top, one for bottom, the other for the sides around it.
        /// </summary>
        public static TextureLayout UnqiueColumn(string sides, string bottom, string top)
        {
            int sideIndex = _blockTextureIndexProvider.GetTextureIndex(sides);
            int bottomIndex = _blockTextureIndexProvider.GetTextureIndex(bottom);
            int topIndex = _blockTextureIndexProvider.GetTextureIndex(top);

            return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, bottomIndex, topIndex);
        }

        /// <summary>
        /// Returns a texture layout where all sides but the front have the same texture.
        /// </summary>
        public static TextureLayout UnqiueFront(string front, string rest)
        {
            int frontIndex = _blockTextureIndexProvider.GetTextureIndex(front);
            int restIndex = _blockTextureIndexProvider.GetTextureIndex(rest);

            return new TextureLayout(frontIndex, restIndex, restIndex, restIndex, restIndex, restIndex);
        }

        /// <summary>
        /// Returns a texture layout where all sides but the top side have the same texture.
        /// </summary>
        public static TextureLayout UnqiueTop(string rest, string top)
        {
            int topIndex = _blockTextureIndexProvider.GetTextureIndex(top);
            int restIndex = _blockTextureIndexProvider.GetTextureIndex(rest);

            return new TextureLayout(restIndex, restIndex, restIndex, restIndex, restIndex, topIndex);
        }

        /// <summary>
        /// Returns a texture layout using liquid textures. The layout itself is similar to <see cref="TextureLayout.Column(string, string)"/>.
        /// </summary>
        public static TextureLayout Liquid(string sides, string ends)
        {
            int sideIndex = _liquidTextureIndexProvider.GetTextureIndex(sides);
            int endIndex = _liquidTextureIndexProvider.GetTextureIndex(ends);

            return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, endIndex, endIndex);
        }

        public int[][] GetTexIndexArrays()
        {
            return new[]
            {
                new[]
                {
                    Front, Front, Front, Front
                },
                new[]
                {
                    Back, Back, Back, Back
                },
                new[]
                {
                    Left, Left, Left, Left
                },
                new[]
                {
                    Right, Right, Right, Right
                },
                new[]
                {
                    Bottom, Bottom, Bottom, Bottom
                },
                new[]
                {
                    Top, Top, Top, Top
                }
            };
        }

        public readonly int[] GetTexIndexArray()
        {
            return new[]
            {
                Front,
                Back,
                Left,
                Right,
                Bottom,
                Top
            };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Front, Back, Left, Right, Bottom, Top);
        }

        public static bool operator ==(TextureLayout left, TextureLayout right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureLayout left, TextureLayout right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is TextureLayout other)
            {
                return Equals(other: other);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(TextureLayout other)
        {
            return Front == other.Front &&
                   Back == other.Back &&
                   Left == other.Left &&
                   Right == other.Right &&
                   Bottom == other.Bottom &&
                   Top == other.Top;
        }
    }
}