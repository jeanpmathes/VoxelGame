// <copyright file="GenerationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Generation.Worlds.Water;

/// <summary>
///     Implementation of <see cref="IGenerationContext" />.
/// </summary>
public sealed class GenerationContext(Generator generator) : BaseGenerationContext(generator)
{
    /// <inheritdoc />
    public override IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        return generator.GenerateColumn(x, z, heightRange);
    }
}
