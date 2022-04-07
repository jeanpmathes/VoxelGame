// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Generates a world.
/// </summary>
public interface IWorldGenerator
{
    /// <summary>
    ///     Generate a column of the world.
    /// </summary>
    /// <param name="x">The x position of the world.</param>
    /// <param name="z">The y position of the world.</param>
    /// <returns>The blocks in the column.</returns>
    IEnumerable<Block> GenerateColumn(int x, int z);
}
