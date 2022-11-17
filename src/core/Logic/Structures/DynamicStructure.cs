// <copyright file="DynamicStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Logic.Structures;

/// <summary>
///     A dynamic structure is a structure that is generated using a seed.
/// </summary>
public abstract class DynamicStructure : Structure
{
    /// <summary>
    ///     Get the random number generator for this structure.
    /// </summary>
    protected Random Random { get; private set; }

    /// <inheritdoc />
    public override bool IsPlaceable => Random != null;

    /// <summary>
    ///     Set the structure seed.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    public override void SetStructureSeed(int seed)
    {
        Random = new Random(seed);
    }
}
