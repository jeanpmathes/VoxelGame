// <copyright file="Unmeshed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Meshables;

/// <summary>
/// Corresponds to <see cref="Meshable.Simple"/>
/// </summary>
public class Unmeshed : BlockBehavior, IBehavior<Unmeshed, BlockBehavior, Block>, IMeshable
{
    private Unmeshed(Block subject) : base(subject)
    {
        
    }
    
    /// <inheritdoc />
    public Meshable Type => Meshable.Unmeshed;

    /// <inheritdoc />
    public static Unmeshed Construct(Block input)
    {
        return new Unmeshed(input);
    }
    
    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsOpaque.ContributeConstant(value: false);
    }
}
