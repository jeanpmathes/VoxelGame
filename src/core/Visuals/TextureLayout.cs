// <copyright file="TextureLayout.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides functionality to define the textures of a default six-sided block or fluid.
/// </summary>
public readonly struct TextureLayout : IEquatable<TextureLayout>
{
    private static ITextureIndexProvider blockTextureIndexProvider = null!;
    private static ITextureIndexProvider fluidTextureIndexProvider = null!;

    /// <summary>
    ///     Set the texture index providers to get the texture index for a texture name.
    /// </summary>
    /// <param name="blockTextureProvider">The block texture index provider.</param>
    /// <param name="fluidTextureProvider">The fluid texture index provider.</param>
    public static void SetProviders(ITextureIndexProvider blockTextureProvider,
        ITextureIndexProvider fluidTextureProvider)
    {
        blockTextureIndexProvider = blockTextureProvider;
        fluidTextureIndexProvider = fluidTextureProvider;
    }

    /// <summary>
    ///     The front texture index.
    /// </summary>
    public Int32 Front { get; }

    /// <summary>
    ///     The back texture index.
    /// </summary>
    public Int32 Back { get; }

    /// <summary>
    ///     The left texture index.
    /// </summary>
    public Int32 Left { get; }

    /// <summary>
    ///     The right texture index.
    /// </summary>
    public Int32 Right { get; }

    /// <summary>
    ///     The bottom texture index.
    /// </summary>
    public Int32 Bottom { get; }

    /// <summary>
    ///     The top texture index.
    /// </summary>
    public Int32 Top { get; }

    /// <summary>
    ///     Create a new texture layout, with all texture indices directly set to a specific value.
    /// </summary>
    public TextureLayout(Int32 front, Int32 back, Int32 left, Int32 right, Int32 bottom, Int32 top)
    {
        Front = front;
        Back = back;
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
    }

    /// <summary>
    ///     Returns a texture layout where every side has the same texture.
    /// </summary>
    public static TextureLayout Uniform(String texture)
    {
        Int32 i = blockTextureIndexProvider.GetTextureIndex(texture);

        return new TextureLayout(i, i, i, i, i, i);
    }

    /// <summary>
    ///     Returns a texture layout where every side has a different texture.
    /// </summary>
    public static TextureLayout Unique(String front, String back, String left, String right, String bottom,
        String top)
    {
        return new TextureLayout(
            blockTextureIndexProvider.GetTextureIndex(front),
            blockTextureIndexProvider.GetTextureIndex(back),
            blockTextureIndexProvider.GetTextureIndex(left),
            blockTextureIndexProvider.GetTextureIndex(right),
            blockTextureIndexProvider.GetTextureIndex(bottom),
            blockTextureIndexProvider.GetTextureIndex(top));
    }

    /// <summary>
    ///     Returns a texture layout where two textures are used, one for top/bottom, the other for the sides around it.
    /// </summary>
    public static TextureLayout Column(String sides, String ends)
    {
        Int32 sideIndex = blockTextureIndexProvider.GetTextureIndex(sides);
        Int32 endIndex = blockTextureIndexProvider.GetTextureIndex(ends);

        return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, endIndex, endIndex);
    }

    /// <summary>
    ///     Returns a texture layout where two textures are used, one for top/bottom, the other for the sides around it.
    /// </summary>
    public static TextureLayout Column(String texture, Int32 sideOffset, Int32 endOffset)
    {
        Int32 sideIndex = blockTextureIndexProvider.GetTextureIndex(texture) + sideOffset;
        Int32 endIndex = blockTextureIndexProvider.GetTextureIndex(texture) + endOffset;

        return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, endIndex, endIndex);
    }

    /// <summary>
    ///     Returns a texture layout where three textures are used, one for top, one for bottom, the other for the sides around
    ///     it.
    /// </summary>
    public static TextureLayout UniqueColumn(String sides, String bottom, String top)
    {
        Int32 sideIndex = blockTextureIndexProvider.GetTextureIndex(sides);
        Int32 bottomIndex = blockTextureIndexProvider.GetTextureIndex(bottom);
        Int32 topIndex = blockTextureIndexProvider.GetTextureIndex(top);

        return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, bottomIndex, topIndex);
    }

    /// <summary>
    ///     Returns a texture layout where all sides but the front have the same texture.
    /// </summary>
    public static TextureLayout UniqueFront(String front, String rest)
    {
        Int32 frontIndex = blockTextureIndexProvider.GetTextureIndex(front);
        Int32 restIndex = blockTextureIndexProvider.GetTextureIndex(rest);

        return new TextureLayout(frontIndex, restIndex, restIndex, restIndex, restIndex, restIndex);
    }

    /// <summary>
    ///     Returns a texture layout where all sides but the top side have the same texture.
    /// </summary>
    public static TextureLayout UniqueTop(String rest, String top)
    {
        Int32 topIndex = blockTextureIndexProvider.GetTextureIndex(top);
        Int32 restIndex = blockTextureIndexProvider.GetTextureIndex(rest);

        return new TextureLayout(restIndex, restIndex, restIndex, restIndex, restIndex, topIndex);
    }

    /// <summary>
    ///     Returns a texture layout using fluid textures. The layout itself is similar to
    ///     <see cref="TextureLayout.Column(string, string)" />.
    /// </summary>
    public static TextureLayout Fluid(String sides, String ends)
    {
        Int32 sideIndex = fluidTextureIndexProvider.GetTextureIndex(sides);
        Int32 endIndex = fluidTextureIndexProvider.GetTextureIndex(ends);

        return new TextureLayout(sideIndex, sideIndex, sideIndex, sideIndex, endIndex, endIndex);
    }

    /// <summary>
    ///     Get the texture index array for the given texture layout.
    /// </summary>
    /// <returns>
    ///     The texture index array. The array is of length 6, with the indices in the side order defined by
    ///     <see cref="BlockSide" />.
    /// </returns>
    public Int32[] GetTextureIndexArray()
    {
        return
        [
            Front,
            Back,
            Left,
            Right,
            Bottom,
            Top
        ];
    }

    /// <summary>
    /// </summary>
    /// <param name="layout"></param>
    /// <returns></returns>
    public static implicit operator (Int32, Int32, Int32, Int32, Int32, Int32)(TextureLayout layout)
    {
        return layout.ToValueTuple();
    }

    /// <summary>
    ///     Get this texture layout as a value tuple.
    /// </summary>
    /// <returns>The tuple containing the texture numbers.</returns>
    public (Int32, Int32, Int32, Int32, Int32, Int32) ToValueTuple()
    {
        return (Front, Back, Left, Right, Bottom, Top);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Front, Back, Left, Right, Bottom, Top);
    }

    /// <summary>
    ///     Check equality between two texture layouts.
    /// </summary>
    public static Boolean operator ==(TextureLayout left, TextureLayout right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Check inequality between two texture layouts.
    /// </summary>
    public static Boolean operator !=(TextureLayout left, TextureLayout right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is TextureLayout other) return Equals(other);

        return false;
    }

    /// <inheritdoc />
    public Boolean Equals(TextureLayout other)
    {
        return ToValueTuple() == other.ToValueTuple();
    }
}
