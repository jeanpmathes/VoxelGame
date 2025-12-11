// <copyright file="Meshing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Describes how data is encoded for the shaders.
///     See the wiki (https://github.com/jeanpmathes/VoxelGame/wiki/Section-Buffers) for more information.
/// </summary>
public static class Meshing
{
    /// <summary>
    ///     Special flags that are only used for foliage.
    /// </summary>
    public enum FoliageQuadFlag
    {
        /// <summary>
        ///     Whether the current quad is in the upper part of a double plant.
        ///     If this is not a double plant, this flag must be set to false.
        /// </summary>
        IsUpperPart = 0,

        /// <summary>
        ///     Whether the current quad is part of a double plant.
        /// </summary>
        IsDoublePlant = 1
    }

    /// <summary>
    ///     Different flags for a quad.
    /// </summary>
    public enum QuadFlag
    {
        /// <summary>
        ///     Whether the quad is animated.
        /// </summary>
        IsAnimated = 0,

        /// <summary>
        ///     Whether the quad texture is rotated.
        /// </summary>
        IsTextureRotated = 1,

        /// <summary>
        ///     Whether the quad does not receive shading.
        /// </summary>
        IsUnshaded = 2,

        /// <summary>
        ///     Whether the normal of the quad should be inverted.
        /// </summary>
        IsNormalInverted = 3
    }

    private const Int32 UVShift = 15;

    private const Int32 BitsPerTextureIndex = 13;
    private const Int32 BitsPerFluidTextureIndex = 11;

    /// <summary>
    ///     The maximum amount of textures that can be used.
    /// </summary>
    public const Int32 MaxTextureCount = 1 << BitsPerTextureIndex;

    /// <summary>
    ///     The maximum amount of fluid textures that can be used.
    /// </summary>
    public const Int32 MaxFluidTextureCount = 1 << BitsPerFluidTextureIndex;

    private static readonly UInt32 uvMask = BitTools.GetMask(32 - UVShift) << UVShift;

    private static readonly UInt32 textureIndexMask = BitTools.GetMask(Math.Max(BitsPerTextureIndex, BitsPerFluidTextureIndex));

    /// <summary>
    ///     Encode a vector in base 17, assuming all components are in the range [0, 1].
    /// </summary>
    private static UInt32 EncodeInBase17(Vector4 vector)
    {
        UInt32 x = MathTools.RoundedToUInt(vector.X * 16);
        UInt32 y = MathTools.RoundedToUInt(vector.Y * 16);
        UInt32 z = MathTools.RoundedToUInt(vector.Z * 16);
        UInt32 w = MathTools.RoundedToUInt(vector.W * 16);

        return w * 17 * 17 * 17 +
               z * 17 * 17 +
               y * 17 +
               x;
    }

    private static Vector4 DecodeFromBase17(UInt32 value)
    {
        UInt32 x = value % 17;
        UInt32 y = value / 17 % 17;
        UInt32 z = value / 17 / 17 % 17;
        UInt32 w = value / 17 / 17 / 17 % 17;

        return new Vector4(x, y, z, w) / 16;
    }

    /// <summary>
    ///     Set the UV coordinates for a quad.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    /// <param name="uvA">The UV coordinate of the first vertex.</param>
    /// <param name="uvB">The UV coordinate of the second vertex.</param>
    /// <param name="uvC">The UV coordinate of the third vertex.</param>
    /// <param name="uvD">The UV coordinate of the fourth vertex.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetUVs(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data,
        Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
    {
        data.c &= ~uvMask;
        data.c |= EncodeInBase17((uvA.X, uvB.X, uvC.X, uvD.X)) << UVShift;

        data.d &= ~uvMask;
        data.d |= EncodeInBase17((uvA.Y, uvB.Y, uvC.Y, uvD.Y)) << UVShift;
    }

    /// <summary>
    ///     Get the UV coordinates for a quad.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    /// <returns>The UV coordinates of the quad.</returns>
    public static (Vector2 a, Vector2 b, Vector2 c, Vector2 d) GetUVs(
        ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data)
    {
        Vector4 u = DecodeFromBase17((data.c & uvMask) >> UVShift);
        Vector4 v = DecodeFromBase17((data.d & uvMask) >> UVShift);

        return (new Vector2(u.X, v.X), new Vector2(u.Y, v.Y), new Vector2(u.Z, v.Z), new Vector2(u.W, v.W));
    }

    /// <summary>
    ///     Mirror UVs that are already set on the U axis.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    public static void MirrorUVs(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data)
    {
        Vector4 u = DecodeFromBase17((data.c & uvMask) >> UVShift);

        data.c &= ~uvMask;
        data.c |= EncodeInBase17((u.W, u.Z, u.Y, u.X)) << UVShift;
    }

    /// <summary>
    ///     Set full UV coordinates for a quad, meaning that the quad will use the whole texture.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    /// <param name="mirror">Whether the texture should be mirrored along the U axis.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFullUVs(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, Boolean mirror = false)
    {
        if (mirror) SetUVs(ref data, (1, 0), (1, 1), (0, 1), (0, 0));
        else SetUVs(ref data, (0, 0), (0, 1), (1, 1), (1, 0));
    }

    /// <summary>
    ///     Set the texture repetition for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTextureRepetition(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, Boolean isRotated, UInt32 height, UInt32 length)
    {
        const Int32 heightShift = 0;
        const Int32 lengthShift = 4;

        UInt32 repetition = !isRotated
            ? (height << heightShift) | (length << lengthShift)
            : (length << heightShift) | (height << lengthShift);

        data.c |= repetition;
    }

    /// <summary>
    ///     Set a foliage flag for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFoliageFlag(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, FoliageQuadFlag flag, Boolean value)
    {
        data.c |= value.ToUInt() << (Int32) flag;
    }

    /// <summary>
    ///     Set the texture index for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTextureIndex(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, Int32 index)
    {
        data.a |= (UInt32) index & textureIndexMask;
    }

    /// <summary>
    ///     Get the texture index for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 GetTextureIndex(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data)
    {
        return (Int32) (data.a & textureIndexMask);
    }

    /// <summary>
    ///     Set the tint for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTint(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, ColorS tint)
    {
        Debug.Assert(!tint.IsNeutral);

        const Int32 tintShift = 32 - ColorS.TintPrecision * 3;
        data.b |= tint.ToBits() << tintShift;
    }

    /// <summary>
    ///     Set a flag for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlag(ref (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data, QuadFlag flag, Boolean value)
    {
        var shift = (Int32) flag;

        if (value) data.b |= 1u << shift;
        else data.b &= ~(1u << shift);
    }
}
