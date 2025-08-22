// <copyright file="Animated.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Makes a block use animated textures.
/// </summary>
public class Animated : BlockBehavior, IBehavior<Animated, BlockBehavior, Block>
{
    private Animated(Block subject) : base(subject)
    {
        subject.Require<Meshed>().IsAnimated.ContributeConstant(value: true);
    }
    
    /// <inheritdoc/>
    public static Animated Construct(Block input)
    {
        return new Animated(input);
    }
}
