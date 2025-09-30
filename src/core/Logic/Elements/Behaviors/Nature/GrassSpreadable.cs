// <copyright file="GrassSpreadable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// Blocks which can receive grass spread from a <see cref="Grass"/> block.
/// </summary>
public class GrassSpreadable : BlockBehavior, IBehavior<GrassSpreadable, BlockBehavior, Block>
{
    private GrassSpreadable(Block subject) : base(subject) {}
    
    /// <inheritdoc/>
    public static GrassSpreadable Construct(Block input)
    {
        return new GrassSpreadable(input);
    }
    
    /// <summary>
    ///     Spreads grass on the block. This operation does not always succeed.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="grass">The grass block that is spreading.</param>
    /// <returns>True when the grass block successfully spread.</returns>
    public Boolean SpreadGrass(World world, Vector3i position, Block grass)
    {
        if (world.GetBlock(position)?.Block != Subject || CoveredSoil.CanHaveCover(world, position) != false) return false;

        world.SetBlock(new State(grass), position);

        return true;
    }
}
