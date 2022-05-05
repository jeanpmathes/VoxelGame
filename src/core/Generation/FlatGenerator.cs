// <copyright file="FlatGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Generates a completely flat world.
/// </summary>
public class FlatGenerator : IWorldGenerator
{
    private readonly int heightAir;
    private readonly int heightDirt;

    /// <summary>
    ///     Creates a new flat world generator.
    /// </summary>
    /// <param name="heightAir">The height at which air starts.</param>
    /// <param name="heightDirt">The height at which dirt is used.</param>
    public FlatGenerator(int heightAir, int heightDirt)
    {
        this.heightAir = heightAir;
        this.heightDirt = heightDirt;
    }

    /// <inheritdoc />
    public IEnumerable<Block> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        for (int y = heightRange.start; y < heightRange.end; y++)
            if (y > heightAir) yield return Block.Air;
            else if (y == heightAir) yield return Block.Grass;
            else if (y > heightDirt) yield return Block.Dirt;
            else yield return Block.Stone;
    }
}
