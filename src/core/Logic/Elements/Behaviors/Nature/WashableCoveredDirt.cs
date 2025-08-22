// <copyright file="WashableCoveredDirt.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// Extends <see cref="CoveredDirt"/> to remove the cover when becoming wet.
/// </summary>
public class WashableCoveredDirt : BlockBehavior, IBehavior<WashableCoveredDirt, BlockBehavior, Block>
{
    private readonly CoveredDirt dirt;
    
    private WashableCoveredDirt(Block subject) : base(subject)
    {
        subject.Require<Wet>();
        
        dirt = subject.Require<CoveredDirt>();
    }
    
    /// <inheritdoc/>
    public static WashableCoveredDirt Construct(Block input)
    {
        return new WashableCoveredDirt(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Wet.BecomeWetMessage>(OnBecomeWet);
    }
    
    private void OnBecomeWet(Wet.BecomeWetMessage message)
    {
        dirt.RemoveCover(message.World, message.Position);
    }
}
