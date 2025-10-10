// <copyright file="NeutralTint.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Gives a block a <see cref="ColorS.Neutral" /> tint.
/// </summary>
public class NeutralTint : BlockBehavior, IBehavior<NeutralTint, BlockBehavior, Block>
{
    private NeutralTint(Block subject) : base(subject)
    {
        subject.Require<Meshed>().Tint.ContributeConstant(ColorS.Neutral);
    }

    /// <inheritdoc />
    public static NeutralTint Construct(Block input)
    {
        return new NeutralTint(input);
    }
}
