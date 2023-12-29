// <copyright file="Meshing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;

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
        IsTextureRotated = 1
    }

    private const int UVShift = 15;
    private static readonly uint uvMask = BitHelper.GetMask(32 - UVShift) << UVShift;

    /// <summary>
    ///     Encode a vector in base 17, assuming all components are in the range [0, 1].
    /// </summary>
    private static uint EncodeInBase17(Vector4 vector)
    {
        uint x = VMath.RoundedToUInt(vector.X * 16);
        uint y = VMath.RoundedToUInt(vector.Y * 16);
        uint z = VMath.RoundedToUInt(vector.Z * 16);
        uint w = VMath.RoundedToUInt(vector.W * 16);

        return w * 17 * 17 * 17 +
               z * 17 * 17 +
               y * 17 +
               x;
    }

    private static Vector4 DecodeFromBase17(uint value)
    {
        uint x = value % 17;
        uint y = value / 17 % 17;
        uint z = value / 17 / 17 % 17;
        uint w = value / 17 / 17 / 17 % 17;

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
    public static void SetUVs(ref (uint a, uint b, uint c, uint d) data,
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
        ref (uint a, uint b, uint c, uint d) data)
    {
        Vector4 u = DecodeFromBase17((data.c & uvMask) >> UVShift);
        Vector4 v = DecodeFromBase17((data.d & uvMask) >> UVShift);

        return (new Vector2(u.X, v.X), new Vector2(u.Y, v.Y), new Vector2(u.Z, v.Z), new Vector2(u.W, v.W));
    }

    /// <summary>
    ///     Mirror UVs that are already set on the U axis.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    public static void MirrorUVs(ref (uint a, uint b, uint c, uint d) data)
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
    public static void SetFullUVs(ref (uint a, uint b, uint c, uint d) data, bool mirror = false)
    {
        if (mirror) SetUVs(ref data, (1, 0), (1, 1), (0, 1), (0, 0));
        else SetUVs(ref data, (0, 0), (0, 1), (1, 1), (1, 0));
    }

    /// <summary>
    ///     Set the texture repetition for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTextureRepetition(ref (uint a, uint b, uint c, uint d) data, bool isRotated, uint height, uint length)
    {
        const int heightShift = 0;
        const int lengthShift = 4;

        uint repetition = !isRotated
            ? (height << heightShift) | (length << lengthShift)
            : (length << heightShift) | (height << lengthShift);

        data.c |= repetition;
    }

    /// <summary>
    ///     Set a foliage flag for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFoliageFlag(ref (uint a, uint b, uint c, uint d) data, FoliageQuadFlag flag, bool value)
    {
        data.c |= value.ToUInt() << (int) flag;
    }

    /// <summary>
    ///     Set the texture index for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTextureIndex(ref (uint a, uint b, uint c, uint d) data, int index)
    {
        const int indexMask = (1 << 13) - 1;
        data.a |= (uint) index & indexMask;
    }

    /// <summary>
    ///     Set the tint for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTint(ref (uint a, uint b, uint c, uint d) data, TintColor tint)
    {
        Debug.Assert(!tint.IsNeutral);

        const int tintShift = 23;
        data.b |= tint.ToBits << tintShift;
    }

    /// <summary>
    ///     Set a flag for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlag(ref (uint a, uint b, uint c, uint d) data, QuadFlag flag, bool value)
    {
        data.b |= value.ToUInt() << (int) flag;
    }
}
