// <copyright file="Static.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     Prevents placement and destruction of the block.
///     Note that if the block is replaceable, it can still be replaced by other blocks.
/// </summary>
public class Static : BlockBehavior, IBehavior<Static, BlockBehavior, Block>
{
    private Static(Block subject) : base(subject)
    {
        subject.IsPlacementAllowed.ContributeConstant(value: false);
        subject.IsDestructionAllowed.ContributeConstant(value: false);
    }

    /// <inheritdoc />
    public static Static Construct(Block input)
    {
        return new Static(input);
    }
}
