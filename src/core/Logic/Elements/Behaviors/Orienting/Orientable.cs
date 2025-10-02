// <copyright file="Orientable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
/// Blocks that can be oriented in some way, e.g. by rotation or siding.
/// </summary>
public class Orientable : BlockBehavior, IBehavior<Orientable, BlockBehavior, Block>
{
    private Orientable(Block subject) : base(subject) {}
    
    /// <inheritdoc />
    public static Orientable Construct(Block input)
    {
        return new Orientable(input);
    }
}
