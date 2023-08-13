// <copyright file="IHeightVariable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows blocks to have variable height.
///     The height varies in steps of 1/16th of a block.
///     Height is a number from 0 to <see cref="MaximumHeight" /> inclusive.
/// </summary>
public interface IHeightVariable : IBlockBase
{
    /// <summary>
    ///     The maximum height. A block with this height completely fills a position.
    /// </summary>
    public static int MaximumHeight => 15;

    /// <summary>
    ///     The half height. A block with this height fills half of a position.
    /// </summary>
    public static int HalfHeight => MaximumHeight / 2;

    /// <inheritdoc />
    bool IBlockBase.IsSideFull(BlockSide side, uint data)
    {
        if (side == BlockSide.Bottom) return true;

        int height = GetHeight(data);

        return height == MaximumHeight;
    }

    /// <summary>
    ///     Get the size of a face with a given height, in world units.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The size of the face.</returns>
    public static float GetSize(int height)
    {
        return (height + 1) / (float) (MaximumHeight + 1);
    }

    /// <summary>
    ///     Get the gap of a face, which is the space between the end of the face and the end of the block, in world units.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The gap of the face.</returns>
    public static float GetGap(int height)
    {
        return 1 - GetSize(height);
    }

    /// <summary>
    ///     Get the bounds of a face with a given height.
    ///     The bounds can be used as texture coordinates.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The bounds of the face.</returns>
    public static (Vector2 min, Vector2 max) GetBounds(int height)
    {
        float size = GetSize(height);

        return (new Vector2(x: 0, y: 0), new Vector2(x: 1, size));
    }

    /// <summary>
    ///     Get the height of a block, given the block data.
    /// </summary>
    /// <param name="data">The data from which to extract the height.</param>
    /// <returns>The block height.</returns>
    int GetHeight(uint data);
}
