// <copyright file="Animated.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Makes a block use animated textures.
/// </summary>
public partial class Animated : BlockBehavior, IBehavior<Animated, BlockBehavior, Block>
{
    [Constructible]
    private Animated(Block subject) : base(subject)
    {
        subject.Require<Meshed>().IsAnimated.ContributeConstant(value: true);
    }
}
