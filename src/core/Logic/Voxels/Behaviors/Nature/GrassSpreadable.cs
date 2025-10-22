// <copyright file="GrassSpreadable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Blocks which can receive grass spread from a <see cref="Grass" /> block.
/// </summary>
public partial class GrassSpreadable : BlockBehavior, IBehavior<GrassSpreadable, BlockBehavior, Block>
{
    [Constructible]
    private GrassSpreadable(Block subject) : base(subject) {}

    /// <summary>
    ///     Spreads grass on the block. This operation does not always succeed.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="grass">The grass block that is spreading.</param>
    /// <returns>True when the grass block successfully spread.</returns>
    public Boolean SpreadGrass(World world, Vector3i position, Block grass)
    {
        if (world.GetBlock(position)?.Block != Subject || CoveredSoil.CanHaveCover(world, position) != true) return false;

        world.SetBlock(new State(grass), position);

        return true;
    }
}
