// <copyright file="BlockModels.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Utility class for block models.
/// </summary>
public static class BlockModels
{
    /// <summary>
    ///     Create a fallback block model.
    ///     It does not rely on any textures and can be safely used when resources are not available.
    /// </summary>
    /// <returns>The fallback block model.</returns>
    public static BlockModel CreateFallback()
    {
        const Single begin = 0.275f;
        const Single size = 0.5f;

        Int32[][] uvs = BlockMeshes.GetBlockUVs(isRotated: false);

        Quad BuildQuad(BlockSide side)
        {
            Vertex BuildVertex(IReadOnlyList<Int32> corner, IReadOnlyList<Int32> uv)
            {
                return new Vertex
                {
                    X = begin + corner[index: 0] * size,
                    Y = begin + corner[index: 1] * size,
                    Z = begin + corner[index: 2] * size,
                    U = begin + uv[index: 0] * size,
                    V = begin + uv[index: 1] * size
                };
            }

            side.Corners(out Int32[] a, out Int32[] b, out Int32[] c, out Int32[] d);

            return new Quad
            {
                TextureId = 0,
                Vert0 = BuildVertex(a, uvs[0]),
                Vert1 = BuildVertex(b, uvs[1]),
                Vert2 = BuildVertex(c, uvs[2]),
                Vert3 = BuildVertex(d, uvs[3])
            };
        }

        return new BlockModel
        {
            TextureNames = ["missing_texture"],
            Quads =
            [
                BuildQuad(BlockSide.Front),
                BuildQuad(BlockSide.Back),
                BuildQuad(BlockSide.Left),
                BuildQuad(BlockSide.Right),
                BuildQuad(BlockSide.Bottom),
                BuildQuad(BlockSide.Top)
            ]
        };
    }

    /// <summary>
    ///     Lock a group of models.
    /// </summary>
    /// <param name="group">The models to lock.</param>
    public static void Lock(
        this (
            BlockModel front,
            BlockModel back,
            BlockModel left,
            BlockModel right,
            BlockModel bottom,
            BlockModel top)
            group)
    {
        group.front.Lock();
        group.back.Lock();
        group.left.Lock();
        group.right.Lock();
        group.bottom.Lock();
        group.top.Lock();
    }

    /// <summary>
    ///     Lock a group of models.
    /// </summary>
    /// <param name="group">The group to lock.</param>
    public static void Lock(this (BlockModel north, BlockModel east, BlockModel south, BlockModel west) group)
    {
        group.north.Lock();
        group.east.Lock();
        group.south.Lock();
        group.west.Lock();
    }
}
