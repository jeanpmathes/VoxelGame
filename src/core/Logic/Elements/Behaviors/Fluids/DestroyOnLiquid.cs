// <copyright file="DestroyOnLiquid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
/// Breaks when filled with more than a certain amount of liquid.
/// </summary>
public class DestroyOnLiquid  : BlockBehavior, IBehavior<DestroyOnLiquid, BlockBehavior, Block>
{
    private DestroyOnLiquid(Block subject) : base(subject)
    {
        ThresholdInitializer = Aspect<FluidLevel, Block>.New<Exclusive<FluidLevel, Block>>(nameof(ThresholdInitializer), this); 
        // todo: the fluid level struct should be written in a way so that it supports the Minimum strategy being used here instead of Exclusive
        // todo: maybe FluidLevel struct should also have a -1 value for no level but the method to get the UInt32 should assert that this does not get returned
    }
    
    /// <inheritdoc />
    public static DestroyOnLiquid Construct(Block input)
    {
        return new DestroyOnLiquid(input);
    }

    /// <summary>
    /// The threshold above which the block breaks when filled with liquid.
    /// </summary>
    public FluidLevel Threshold { get; private set; } = FluidLevel.One;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Threshold"/> property.
    /// </summary>
    public Aspect<FluidLevel, Block> ThresholdInitializer { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Threshold = ThresholdInitializer.GetValue(FluidLevel.One, Subject);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ContentUpdateMessage>(OnContentUpdate);
    }

    private void OnContentUpdate(Block.ContentUpdateMessage message)
    {
        if (message.NewContent.Fluid.Fluid.IsLiquid && message.NewContent.Fluid.Level > Threshold)
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
