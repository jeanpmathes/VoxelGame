﻿// <copyright file="Layer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A layer of the world.
/// </summary>
public abstract class Layer
{
    /// <summary>
    ///     The width of the layer, in number of blocks.
    /// </summary>
    public int Width { get; protected init; }

    /// <summary>
    ///     Get whether this layer is solid and does not allow water to pass through.
    /// </summary>
    public bool IsSolid { get; protected init; }

    /// <summary>
    /// Create a cover layer, which selects an alternative when filled. The alternative block is also filled with water if possible.
    /// </summary>
    public static Layer CreateCover(IBlockBase cover, IBlockBase filled, int width)
    {
        return new Cover(cover, filled, width);
    }

    /// <summary>
    ///     Create a layer with a permeable material that will be filled with water.
    /// </summary>
    public static Layer CreatePermeable(IBlockBase block, int width)
    {
        return new Permeable(block, width);
    }

    /// <summary>
    ///     Create a solid layer, which always has the same block.
    /// </summary>
    public static Layer CreateSolid(IBlockBase block, int width)
    {
        return new Solid(block, width);
    }

    /// <summary>
    ///     Returns the data for the layer content.
    /// </summary>
    /// <param name="isFilled">Whether the column is filled with fluid or not.</param>
    /// <returns>The data for the layer content.</returns>
    public abstract uint GetData(bool isFilled);

    private sealed class Cover : Layer
    {
        private readonly uint filledData;
        private readonly uint normalData;

        public Cover(IBlockBase cover, IBlockBase filled, int width)
        {
            Width = width;

            normalData = Section.Encode(cover);
            filledData = filled is IFillable ? Section.Encode(filled, Fluid.Water) : Section.Encode(filled);
        }

        public override uint GetData(bool isFilled)
        {
            return isFilled ? filledData : normalData;
        }
    }

    private sealed class Permeable : Layer
    {
        private readonly uint filledData;
        private readonly uint normalData;

        public Permeable(IBlockBase block, int width)
        {
            Width = width;

            normalData = Section.Encode(block);
            filledData = Section.Encode(block, Fluid.Water);
        }

        public override uint GetData(bool isFilled)
        {
            return isFilled ? filledData : normalData;
        }
    }

    private sealed class Solid : Layer
    {
        private readonly uint data;

        public Solid(IBlockBase block, int width)
        {
            Width = width;
            IsSolid = true;

            data = Section.Encode(block);
        }

        public override uint GetData(bool isFilled)
        {
            return data;
        }
    }
}
