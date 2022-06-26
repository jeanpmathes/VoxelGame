// <copyright file="DefaultGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;

namespace VoxelGame.Core.Generation;

/// <summary>
///     The default world generator.
/// </summary>
public class DefaultGenerator : IWorldGenerator
{
    private readonly int seed;

    /// <summary>
    ///     Creates a new default world generator.
    /// </summary>
    /// <param name="seed">The seed to use for generation.</param>
    public DefaultGenerator(int seed)
    {
        this.seed = seed;
    }

    /// <inheritdoc />
    public IEnumerable<uint> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        yield return 0;
    }
}
