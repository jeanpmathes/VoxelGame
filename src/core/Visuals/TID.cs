// <copyright file="TID.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Visuals;

#pragma warning disable S101 // Full name would be to long for this commonly used type.

/// <summary>
///     A texture identifier.
/// </summary>
public readonly struct TID : IEquatable<TID>
{
    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    public const String MissingTextureKey = "missing_texture";

    /// <summary>
    ///     The zero offset.
    /// </summary>
    public const Byte ZeroOffset = 0;

    private readonly String baseKey = MissingTextureKey;
    private readonly Byte offset = ZeroOffset;

    /// <summary>
    ///     Gets the key of the texture.
    /// </summary>
    public String Key => offset == ZeroOffset ? baseKey : $"{baseKey}:{offset}";

    /// <summary>
    ///     Whether the texture is a block texture or a fluid texture.
    /// </summary>
    public Boolean IsBlock { get; } = true;

    /// <summary>
    ///     Creates a new texture identifier referring to the missing texture.
    /// </summary>
    public static TID MissingTexture => new(MissingTextureKey, offset: 0, isBlock: true);

    private TID(String baseKey, Byte offset, Boolean isBlock)
    {
        this.baseKey = baseKey;
        this.offset = offset;

        IsBlock = isBlock;
    }

    /// <summary>
    ///     Create a block texture identifier.
    /// </summary>
    /// <param name="key">The key of the texture.</param>
    /// <returns>The texture identifier.</returns>
    public static TID Block(String key)
    {
        return new TID(key, offset: 0, isBlock: true);
    }

    /// <summary>
    ///     Create a block texture identifier with an offset.
    /// </summary>
    /// <param name="key">The key of the texture.</param>
    /// <param name="offset">The offset of the texture.</param>
    /// <returns>The texture identifier.</returns>
    public static TID Block(String key, Byte offset)
    {
        return new TID(key, offset, isBlock: true);
    }

    /// <summary>
    ///     Create a fluid texture identifier.
    /// </summary>
    /// <param name="key">The key of the texture.</param>
    /// <returns>The texture identifier.</returns>
    public static TID Fluid(String key)
    {
        return new TID(key, offset: 0, isBlock: false);
    }

    /// <summary>
    ///     Whether this identifier refers to the missing texture.
    /// </summary>
    public Boolean IsMissingTexture => baseKey == MissingTextureKey;

    /// <inheritdoc />
    public override String ToString()
    {
        return IsBlock ? $"block:{Key}" : $"fluid:{Key}";
    }

    /// <summary>
    ///     Creates a key identifying a texture with an offset.
    /// </summary>
    /// <param name="key">The base key of the texture.</param>
    /// <param name="offset">The offset of the texture.</param>
    /// <returns>The key identifying the texture with the offset.</returns>
    public static String CreateKey(String key, Byte offset)
    {
        TID tid = new(key, offset, isBlock: true);

        return tid.Key;
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(TID other)
    {
        return baseKey == other.baseKey && offset == other.offset && IsBlock == other.IsBlock;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is TID other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(baseKey, offset, IsBlock);
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

    #endregion EQUALITY
}
