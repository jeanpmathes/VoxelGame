// <copyright file="Sided.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Siding;

public class Sided  : BlockBehavior, IBehavior<Sided, BlockBehavior, Block>
{ 
    // todo: implement as glue for Single and Multi Sided, returns side flags
    
    private Sided(Block subject) : base(subject) {}
    
    /// <inheritdoc/>
    public static Sided Construct(Block input)
    {
        return new Sided(input);
    }
}
