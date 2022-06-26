// <copyright file="DefaultGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Generation;

/// <summary>
///     The default world generator.
/// </summary>
public class DefaultGenerator : IWorldGenerator
{
    private const int SeaLevel = 0;

    private readonly Palette palette = new();

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
        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateBlock((x, y, z));
    }

    private uint GenerateBlock(Vector3i position)
    {
        if (position.Y <= SeaLevel) return palette.Water;

        return palette.Empty;
    }
}
