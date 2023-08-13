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

    /// <summary>
    ///     Push a quad to a mesh.
    /// </summary>
    /// <param name="mesh">The mesh, defined by vertices.</param>
    /// <param name="positions">The four positions of the quad, in clockwise order.</param>
    /// <param name="data">The data of the quad.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushQuad(
        PooledList<SpatialVertex> mesh,
        in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions,
        in (uint a, uint b, uint c, uint d) data)
    {
        mesh.Add(new SpatialVertex
        {
            Position = positions.a,
            Data = data.a
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.b,
            Data = data.b
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.c,
            Data = data.c
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.d,
            Data = data.d
        });
    }

    /// <summary>
    ///     Push a quad to a mesh, while applying modifications to the positions and data.
    /// </summary>
    /// <param name="mesh">The mesh, defined by vertices.</param>
    /// <param name="positions">The four positions of the quad, in clockwise order.</param>
    /// <param name="data">The data of the quad.</param>
    /// <param name="offset">The offset to apply to the positions.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushQuadWithOffset(
        PooledList<SpatialVertex> mesh,
        in (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions,
        in (uint a, uint b, uint c, uint d) data,
        Vector3 offset)
    {
        mesh.Add(new SpatialVertex
        {
            Position = positions.a + offset,
            Data = data.a
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.b + offset,
            Data = data.b
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.c + offset,
            Data = data.c
        });

        mesh.Add(new SpatialVertex
        {
            Position = positions.d + offset,
            Data = data.d
        });
    }

    /// <summary>
    ///     Encode a vector in base 17, assuming all components are in the range [0, 1].
    /// </summary>
    private static uint EncodeInBase17(Vector4 vector)
    {
        var x = (uint) (vector.X * 16);
        var y = (uint) (vector.Y * 16);
        var z = (uint) (vector.Z * 16);
        var w = (uint) (vector.W * 16);

        return w * 17 * 17 * 17 +
               z * 17 * 17 +
               y * 17 +
               x;
    }

    private static Vector4 DecodeFromBase17(uint value)
    {
        uint x = value % 17;
        uint y = value / 17 % 17;
        uint z = value / (17 * 17) % 17;
        uint w = value / (17 * 17 * 17) % 17;

        return new Vector4(x / 16f, y / 16f, z / 16f, w / 16f);
    }

    /// <summary>
    ///     Set the UV coordinates for a quad.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    /// <param name="uv0">The UV coordinate of the first vertex.</param>
    /// <param name="uv1">The UV coordinate of the second vertex.</param>
    /// <param name="uv2">The UV coordinate of the third vertex.</param>
    /// <param name="uv3">The UV coordinate of the fourth vertex.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetUVs(ref (uint a, uint b, uint c, uint d) data,
        Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        data.c |= EncodeInBase17((uv0.X, uv1.X, uv2.X, uv3.X)) << UVShift;
        data.d |= EncodeInBase17((uv0.Y, uv1.Y, uv2.Y, uv3.Y)) << UVShift;
    }

    /// <summary>
    ///     Mirror UVs that are already set on the U axis.
    /// </summary>
    /// <param name="data">The data of the quad.</param>
    public static void MirrorUVs(ref (uint a, uint b, uint c, uint d) data)
    {
        uint uvMask = BitHelper.GetMask(32 - UVShift) << UVShift;

        Vector4 u = DecodeFromBase17(data.c >> UVShift);

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
