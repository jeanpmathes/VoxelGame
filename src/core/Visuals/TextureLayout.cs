﻿// <copyright file="TextureLayout.cs" company="VoxelGame">
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
public class TextureLayout(String front, String back, String left, String right, String bottom, String top)
{
    /// <summary>
    ///     Returns a texture layout where every side has the same texture.
    /// </summary>
    public static TextureLayout Uniform(String texture)
    {
        return new TextureLayout(texture, texture, texture, texture, texture, texture);
    }

    /// <summary>
    ///     Returns a texture layout where every side has a different texture.
    /// </summary>
    public static TextureLayout Unique(
        String front,
        String back,
        String left,
        String right,
        String bottom,
        String top)
    {
        return new TextureLayout(front, back, left, right, bottom, top);
    }

    /// <summary>
    ///     Returns a texture layout where two textures are used, one for top/bottom, the other for the sides around it.
    /// </summary>
    public static TextureLayout Column(String sides, String ends)
    {
        return new TextureLayout(sides, sides, sides, sides, ends, ends);
    }

    /// <summary>
    ///     Returns a texture layout where three textures are used, one for top, one for bottom, the other for the sides around
    ///     it.
    /// </summary>
    public static TextureLayout UniqueColumn(String sides, String bottom, String top)
    {
        return new TextureLayout(sides, sides, sides, sides, bottom, top);
    }

    /// <summary>
    ///     Returns a texture layout where all sides but the front have the same texture.
    /// </summary>
    public static TextureLayout UniqueFront(String front, String rest)
    {
        return new TextureLayout(front, rest, rest, rest, rest, rest);
    }

    /// <summary>
    ///     Returns a texture layout where all sides but the top side have the same texture.
    /// </summary>
    public static TextureLayout UniqueTop(String rest, String top)
    {
        return new TextureLayout(rest, rest, rest, rest, rest, top);
    }

    /// <summary>
    ///     Returns a texture layout for fluids. The layout itself is similar to
    ///     <see cref="TextureLayout.Column(string, string)" />.
    /// </summary>
    public static TextureLayout Fluid(String sides, String ends)
    {
        return Column(sides, ends);
    }

    /// <summary>
    ///     Get the texture index array for the given texture layout.
    /// </summary>
    /// <param name="indexProvider">The texture index provider to use.</param>
    /// <returns>
    ///     The texture index array. The array is of length 6, with the indices in the side order defined by
    ///     <see cref="BlockSide" />.
    /// </returns>
    public Int32[] GetTextureIndexArray(ITextureIndexProvider indexProvider)
    {
        var array = new Int32[6];

        array[(Int32) BlockSide.Front] = indexProvider.GetTextureIndex(front);
        array[(Int32) BlockSide.Back] = indexProvider.GetTextureIndex(back);
        array[(Int32) BlockSide.Left] = indexProvider.GetTextureIndex(left);
        array[(Int32) BlockSide.Right] = indexProvider.GetTextureIndex(right);
        array[(Int32) BlockSide.Bottom] = indexProvider.GetTextureIndex(bottom);
        array[(Int32) BlockSide.Top] = indexProvider.GetTextureIndex(top);

        return array;
    }
}
