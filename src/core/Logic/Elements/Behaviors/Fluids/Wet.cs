// <copyright file="Wet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
/// Behavior of blocks that can be wet.
/// </summary>
public class Wet : BlockBehavior, IBehavior<Wet, BlockBehavior, Block>
{
    private Wet(Block subject) : base(subject) {}
    
    /// <inheritdoc/>
    public static Wet Construct(Block input)
    {
        return new Wet(input);
    }

    /// <inheritdoc/>
    public override void DefineEvents(IEventRegistry registry)
    {
        BecomeWet = registry.RegisterEvent<BecomeWetMessage>();
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ContentUpdateMessage>(OnContentUpdate);
    }

    private void OnContentUpdate(Block.ContentUpdateMessage message)
    {
        if (!BecomeWet.HasSubscribers) return;
        
        if (IsWet(message.NewContent.Block.State) || message.NewContent.Fluid.Fluid.IsLiquid)
        {
            BecomeWet.Publish(new BecomeWetMessage(this)
            {
                World = message.World,
                Position = message.Position
            });
        }
    }

    /// <summary>
    /// Get whether the block is wet based on its state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>True if the block is wet, false otherwise.</returns>
    public Boolean IsWet(State state) => state.Fluid?.IsLiquid == true;

    /// <summary>
    /// Sent when a block becomes wet.
    /// </summary>
    public record BecomeWetMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        /// The world in which the block is located.
        /// </summary>
        public World World { get; set; } = null!;
        
        /// <summary>
        /// The position of the block.
        /// </summary>
        public Vector3i Position { get; set; }
    }

    /// <summary>
    /// Called when a block becomes wet.
    /// Could also be called when a block is already wet.
    /// </summary>
    public IEvent<BecomeWetMessage> BecomeWet { get; private set; } = null!;
}
