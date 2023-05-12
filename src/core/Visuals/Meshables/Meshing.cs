// <copyright file="Meshing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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

    /// <summary>
    ///     Push a quad to a mesh, defined by vertices and indices.
    /// </summary>
    /// <param name="vertices">The vertices of the mesh.</param>
    /// <param name="indices">The indices of the mesh.</param>
    /// <param name="positions">The four positions of the quad, in clockwise order.</param>
    /// <param name="data">The data of the quad.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushFaces(
        PooledList<SpatialVertex> vertices, PooledList<uint> indices,
        (Vector3 a, Vector3 b, Vector3 c, Vector3 d) positions,
        (uint a, uint b, uint c, uint d) data)
    {
        var index = (uint) vertices.Count;

        vertices.Add(new SpatialVertex
        {
            Position = positions.a,
            Data = data.a
        });

        vertices.Add(new SpatialVertex
        {
            Position = positions.b,
            Data = data.b
        });

        vertices.Add(new SpatialVertex
        {
            Position = positions.c,
            Data = data.c
        });

        vertices.Add(new SpatialVertex
        {
            Position = positions.d,
            Data = data.d
        });

        indices.Add(index + 0);
        indices.Add(index + 2);
        indices.Add(index + 1);

        indices.Add(index + 0);
        indices.Add(index + 3);
        indices.Add(index + 2);
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
        const int tintShift = 23;
        data.b |= tint.ToBits << tintShift;
    }

    /// <summary>
    ///     Set a flag for a quad.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlag(ref (uint a, uint b, uint c, uint d) data, QuadFlag flag, bool value)
    {
        data.d |= value.ToUInt() << (int) flag;
    }
}
