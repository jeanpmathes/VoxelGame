// <copyright file="DynamicStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Logic.Structures;

/// <summary>
///     A dynamic structure is a structure that is generated using a seed.
/// </summary>
public abstract class DynamicStructure : Structure
{
    private Random? random;

    /// <summary>
    ///     Get the random number generator for this structure.
    /// </summary>
    protected Random Random
    {
        get
        {
            Debug.Assert(IsPlaceable);
            Debug.Assert(random != null);

            return random;
        }
    }

    /// <inheritdoc />
    public override bool IsPlaceable => random != null;

    /// <summary>
    ///     Set the structure seed.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    public override void SetStructureSeed(int seed)
    {
        random = new Random(seed);
    }
}
