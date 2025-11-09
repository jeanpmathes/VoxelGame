// <copyright file="DynamicStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     A dynamic structure is a structure that is generated using a seed.
/// </summary>
public abstract class DynamicStructure : Structure
{
    /// <inheritdoc />
    protected override Random? GetRandomness(Int32 seed)
    {
        return new Random(seed);
    }
}
