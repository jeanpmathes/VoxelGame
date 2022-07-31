// <copyright file="Layer.cs" company="VoxelGame">
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
public class Layer
{
    private readonly uint filledData;

    private readonly uint normalData;

    /// <summary>
    ///     Creates a new layer.
    /// </summary>
    /// <param name="block">The block to fill the layer with.</param>
    /// <param name="width">The width of the layer, in number of blocks.</param>
    public Layer(IBlockBase block, int width)
    {
        Width = width;

        normalData = Section.Encode(block);
        filledData = block is IFillable ? Section.Encode(block, Fluid.Water) : normalData;
    }

    /// <summary>
    ///     The width of the layer, in number of blocks.
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///     Returns the data for the layer content.
    /// </summary>
    /// <param name="isFilled">Whether the layer is filled with fluid or not.</param>
    /// <returns>The data for the layer content.</returns>
    public uint GetData(bool isFilled)
    {
        return isFilled ? filledData : normalData;
    }
}
