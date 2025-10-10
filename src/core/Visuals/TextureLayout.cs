// <copyright file="TextureLayout.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides functionality to define the textures of a default six-sided block or fluid.
/// </summary>
public class TextureLayout(TID front, TID back, TID left, TID right, TID bottom, TID top)
{
    /// <summary>
    ///     Returns a texture layout where every side has the same texture.
    /// </summary>
    public static TextureLayout Uniform(TID texture)
    {
        return new TextureLayout(texture, texture, texture, texture, texture, texture);
    }

    /// <summary>
    ///     Returns a texture layout where every side has a different texture.
    /// </summary>
    public static TextureLayout Unique(
        TID front,
        TID back,
        TID left,
        TID right,
        TID bottom,
        TID top)
    {
        return new TextureLayout(front, back, left, right, bottom, top);
    }

    /// <summary>
    ///     Returns a texture layout where two textures are used, one for top/bottom, the other for the surrounding sides.
    /// </summary>
    public static TextureLayout Column(TID sides, TID ends)
    {
        return new TextureLayout(sides, sides, sides, sides, ends, ends);
    }

    /// <summary>
    ///     Returns a texture layout where three textures are used, one for top, one for bottom, the other for the surrounding
    ///     sides
    ///     .
    /// </summary>
    public static TextureLayout UniqueColumn(TID sides, TID bottom, TID top)
    {
        return new TextureLayout(sides, sides, sides, sides, bottom, top);
    }

    /// <summary>
    ///     Returns a texture layout where all sides but the front have the same texture.
    /// </summary>
    public static TextureLayout UniqueFront(TID front, TID rest)
    {
        return new TextureLayout(front, rest, rest, rest, rest, rest);
    }

    /// <summary>
    ///     Returns a texture layout where all sides but the top side have the same texture.
    /// </summary>
    public static TextureLayout UniqueTop(TID rest, TID top)
    {
        return new TextureLayout(rest, rest, rest, rest, rest, top);
    }

    /// <summary>
    ///     Returns a texture layout for fluids. The layout itself is similar to
    ///     <see cref="TextureLayout.Column(TID, TID)" />.
    /// </summary>
    public static TextureLayout Fluid(TID sides, TID ends)
    {
        return Column(sides, ends);
    }

    private TID GetTexture(Side side)
    {
        return side switch
        {
            Side.Front => front,
            Side.Back => back,
            Side.Left => left,
            Side.Right => right,
            Side.Bottom => bottom,
            Side.Top => top,
            Side.All => throw Exceptions.InvalidOperation("Cannot get texture for all sides."),
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }

    /// <summary>
    ///     Get a rotated version of this texture layout, rotated around the given axis by the given number of 90° clockwise
    ///     turns.
    /// </summary>
    /// <param name="axis">The axis to rotate around.</param>
    /// <param name="turns">The number of 90° clockwise turns to rotate by, must be between 0 and 3 inclusive.</param>
    /// <returns>The rotated texture layout.</returns>
    public TextureLayout Rotated(Axis axis, Int32 turns)
    {
        Debug.Assert(turns is >= 0 and < 4);

        if (turns == 0) return this;

        Side GetRotatedSide(Side side)
        {
            return turns switch
            {
                1 => side.Rotate(axis),
                2 => side.Rotate(axis).Rotate(axis),
                3 => side.Rotate(axis).Rotate(axis).Rotate(axis),
                _ => throw Exceptions.UnsupportedValue(turns)
            };
        }

        return new TextureLayout(
            GetTexture(GetRotatedSide(Side.Front)),
            GetTexture(GetRotatedSide(Side.Back)),
            GetTexture(GetRotatedSide(Side.Left)),
            GetTexture(GetRotatedSide(Side.Right)),
            GetTexture(GetRotatedSide(Side.Bottom)),
            GetTexture(GetRotatedSide(Side.Top))
        );
    }

    /// <summary>
    ///     Get the texture indices that correspond to the textures used by the sides of a block or fluid.
    /// </summary>
    /// <param name="textureIndexProvider">The texture index provider to use.</param>
    /// <param name="isBlock">Whether the texture indices are for a block or a fluid.</param>
    /// <returns>
    ///     The texture indices for the front, back, left, right, bottom, and top sides of a block or fluid.
    /// </returns>
    public SideArray<Int32> GetTextureIndices(ITextureIndexProvider textureIndexProvider, Boolean isBlock)
    {
        SideArray<Int32> sides = new();

        foreach (Side side in Side.All.Sides())
        {
            sides[side] = GetTextureIndex(side, textureIndexProvider, isBlock);
        }

        return sides;
    }

    /// <summary>
    ///     Get the texture index for a specific side of a block or fluid.
    /// </summary>
    /// <param name="side">The side of the block or fluid to get the texture index for, must not be <see cref="Side.All" />.</param>
    /// <param name="textureIndexProvider">The texture index provider to use.</param>
    /// <param name="isBlock">Whether the texture index is for a block or a fluid.</param>
    /// <returns>The texture index for the specified side.</returns>
    public Int32 GetTextureIndex(Side side, ITextureIndexProvider textureIndexProvider, Boolean isBlock)
    {
        return side switch
        {
            Side.Front => textureIndexProvider.GetTextureIndex(front),
            Side.Back => textureIndexProvider.GetTextureIndex(back),
            Side.Left => textureIndexProvider.GetTextureIndex(left),
            Side.Right => textureIndexProvider.GetTextureIndex(right),
            Side.Bottom => textureIndexProvider.GetTextureIndex(bottom),
            Side.Top => textureIndexProvider.GetTextureIndex(top),
            Side.All => throw Exceptions.InvalidOperation("Cannot get texture index for all sides."),
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }
}
