// <copyright file="SixWayRotatable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

public class SixWayRotatable : BlockBehavior, IBehavior<SixWayRotatable, BlockBehavior, Block>
{
    // todo: require Rotatable and contribute to it
    // todo: require SingleSided and contribute to it
    
    private SixWayRotatable(Block subject) : base(subject) {}
    
    /// <inheritdoc />
    public static SixWayRotatable Construct(Block input)
    {
        return new SixWayRotatable(input);
    }
    
    // todo: rotate texture based on the rotation - but only if it has the appropriate behavior so use conditionals
}
