// <copyright file="TID.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VoxelGame.Core.Visuals;

#pragma warning disable S101 // Full name would be to long for this commonly used type.

/// <summary>
///     A texture identifier.
/// </summary>
public readonly partial struct TID : IEquatable<TID>
{
    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    public const String MissingTextureKey = "missing_texture";

    /// <summary>
    ///     The zero offset.
    /// </summary>
    private const Byte ZeroOffset = 0;

    private readonly String baseKey = MissingTextureKey;
    private readonly Byte xOffset = ZeroOffset;
    private readonly Byte yOffset = ZeroOffset;

    /// <summary>
    ///     Gets the key of the texture.
    /// </summary>
    public String Key => $"{baseKey}:{xOffset},{yOffset}";

    /// <summary>
    ///     Whether the texture is a block texture or a fluid texture.
    /// </summary>
    public Boolean IsBlock { get; } = true;

    /// <summary>
    ///     Creates a new texture identifier referring to the missing texture.
    /// </summary>
    public static TID MissingTexture => new(MissingTextureKey, xOffset: 0, yOffset: 0, isBlock: true);

    private TID(String baseKey, Byte xOffset, Byte yOffset, Boolean isBlock)
    {
        Debug.Assert(BaseKeyRegex().IsMatch(baseKey));

        this.baseKey = baseKey;
        this.xOffset = xOffset;
        this.yOffset = yOffset;

        IsBlock = isBlock;
    }

    /// <summary>
    ///     Create a block texture identifier.
    /// </summary>
    /// <param name="key">The key of the texture.</param>
    /// <param name="x">The x offset of the texture in its source.</param>
    /// <param name="y">The y offset of the texture in its source.</param>
    /// <returns>The texture identifier.</returns>
    public static TID Block(String key, Byte x = ZeroOffset, Byte y = ZeroOffset)
    {
        return new TID(key, x, y, isBlock: true);
    }

    /// <summary>
    ///     Create a fluid texture identifier.
    /// </summary>
    /// <param name="key">The key of the texture.</param>
    /// <param name="x">The x offset of the texture in its source.</param>
    /// <param name="y">The y offset of the texture in its source.</param>
    /// <returns>The texture identifier.</returns>
    public static TID Fluid(String key, Byte x = ZeroOffset, Byte y = ZeroOffset)
    {
        return new TID(key, x, y, isBlock: false);
    }

    /// <summary>
    ///     Create a texture identifier from a string.
    ///     The string should be in the format <c>'base_key':'x','y'</c>.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <param name="isBlock">The type of the texture.</param>
    /// <returns>The texture identifier.</returns>
    public static TID FromString(String str, Boolean isBlock)
    {
        ReadOnlySpan<Char> key = str.AsSpan();

        Int32 colonIndex = key.IndexOf(value: ':');

        if (colonIndex == -1)
            return new TID(str, ZeroOffset, ZeroOffset, isBlock);

        ReadOnlySpan<Char> baseKey = key[..colonIndex];

        ReadOnlySpan<Char> offset = key[(colonIndex + 1)..];
        Int32 commaIndex = offset.IndexOf(value: ',');

        Byte xOffset = ZeroOffset;
        Byte yOffset = ZeroOffset;

        if (commaIndex == -1)
        {
            if (Byte.TryParse(offset, out Byte xValue))
                xOffset = xValue;
        }
        else
        {
            ReadOnlySpan<Char> x = offset[..commaIndex];
            ReadOnlySpan<Char> y = offset[(commaIndex + 1)..];

            if (Byte.TryParse(x, out Byte xValue))
                xOffset = xValue;

            if (Byte.TryParse(y, out Byte yValue))
                yOffset = yValue;
        }

        return new TID(baseKey.ToString(), xOffset, yOffset, isBlock);
    }

    /// <summary>
    ///     Whether this identifier refers to the missing texture.
    /// </summary>
    public Boolean IsMissingTexture => baseKey == MissingTextureKey;

    /// <summary>
    ///     Offset from this texture.
    /// </summary>
    /// <param name="x">The x offset. Must remain in the valid range.</param>
    /// <param name="y">The y offset. Must remain in the valid range.</param>
    /// <returns>The new texture identifier.</returns>
    public TID Offset(Byte x = ZeroOffset, Byte y = ZeroOffset)
    {
        Int32 newX = xOffset + x;
        Int32 newY = yOffset + y;

        Debug.Assert(newX <= Byte.MaxValue);
        Debug.Assert(newY <= Byte.MaxValue);

        return new TID(baseKey, (Byte) newX, (Byte) newY, IsBlock);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IsBlock ? $"block:{Key}" : $"fluid:{Key}";
    }

    /// <summary>
    ///     Creates a key identifying a texture with an offset.
    /// </summary>
    /// <param name="key">The base key of the texture.</param>
    /// <param name="x">The x offset of the texture.</param>
    /// <param name="y">The y offset of the texture.</param>
    /// <returns>The key identifying the texture with the offset.</returns>
    public static String CreateKey(String key, Byte x, Byte y)
    {
        TID tid = new(key, x, y, isBlock: true);

        return tid.Key;
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(TID other)
    {
        return baseKey == other.baseKey && xOffset == other.xOffset && yOffset == other.yOffset && IsBlock == other.IsBlock;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is TID other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(baseKey, xOffset, yOffset, IsBlock);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(TID left, TID right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(TID left, TID right)
    {
        return !left.Equals(right);
    }

    [GeneratedRegex(@"^[a-z0-9_]+$")]
    private static partial Regex BaseKeyRegex();

    #endregion EQUALITY
}
