// <copyright file = "Replaceable.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
/// Marks the block as replaceable, see <see cref="Block.Replaceability"/>,
/// </summary>
public partial class Replaceable : BlockBehavior, IBehavior<Replaceable, BlockBehavior, Block>
{
    [Constructible]
    private Replaceable(Block subject) : base(subject)
    {
        subject.Replaceability.ContributeConstant(value: true);
    }
}
