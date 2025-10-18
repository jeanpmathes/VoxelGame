// <copyright file="Static.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     Prevents placement and destruction of the block.
///     Note that if the block is replaceable, it can still be replaced by other blocks.
/// </summary>
public partial class Static : BlockBehavior, IBehavior<Static, BlockBehavior, Block>
{
    [Constructible]
    private Static(Block subject) : base(subject)
    {
        subject.IsPlacementAllowed.ContributeConstant(value: false);
        subject.IsDestructionAllowed.ContributeConstant(value: false);
    }
}
