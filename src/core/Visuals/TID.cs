// <copyright file="TID.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    private const String MissingTextureKey = "missing_texture";

    /// <summary>
    ///     The zero offset.
    /// </summary>
    private const Byte ZeroOffset = 0;

    private readonly Byte xOffset = ZeroOffset;
    private readonly Byte yOffset = ZeroOffset;

    /// <summary>
    ///     Gets the key of the texture.
    /// </summary>
    public String Key => $"{Base}:{xOffset},{yOffset}";

    /// <summary>
    ///     Gets the base key of the texture, without any offsets.
    /// </summary>
    public String Base { get; } = MissingTextureKey;

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

        this.Base = baseKey;
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
    /// <returns>The texture identifier.</returns>
    public static TID Fluid(String key)
    {
        return new TID(key, ZeroOffset, ZeroOffset, isBlock: false);
    }

    /// <summary>
    ///     Whether this identifier refers to the missing texture.
    /// </summary>
    public Boolean IsMissingTexture => Base == MissingTextureKey;

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
        return Base == other.Base && xOffset == other.xOffset && yOffset == other.yOffset && IsBlock == other.IsBlock;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is TID other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Base, xOffset, yOffset, IsBlock);
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
