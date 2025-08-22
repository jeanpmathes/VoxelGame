// <copyright file="Modifiable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Height;

/// <summary>
/// Allows to modify the height by interacting with the block.
/// </summary>
public class Modifiable : BlockBehavior, IBehavior<Modifiable, BlockBehavior, Block>
{
    private Modifiable(Block subject) : base(subject)
    {
        
    }
    
    /// <inheritdoc/>
    public static Modifiable Construct(Block input)
    {
        return new Modifiable(input);
    }

    /// <inheritdoc/>
    public override void DefineEvents(IEventRegistry registry)
    {
        ModifyHeight = registry.RegisterEvent<ModifyHeightMessage>();
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ActorInteractionMessage>(OnActorInteractions);
    }

    private void OnActorInteractions(Block.ActorInteractionMessage message)
    {
        ModifyHeight.Publish(new ModifyHeightMessage(this) // todo: check that subject is correct for all events, should be the actual behavior
        {
            World = message.Actor.World,
            Position = message.Position,
            State = message.State
        }); 
    }

    /// <inheritdoc/>
    protected override void OnValidate(IResourceContext context)
    {
        if (!ModifyHeight.HasSubscribers)
            context.ReportWarning(this, "Modifable behavior has no effect as there are no subsribers");
    }

    /// <summary>
    /// Sent when the height of the block should be modified.
    /// </summary>
    public record ModifyHeightMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        /// The world in which the block is located.
        /// </summary>
        public World World { get; set; } = null!;
        
        /// <summary>
        /// The position of the block in the world.
        /// </summary>
        public Vector3i Position { get; set; }
        
        /// <summary>
        /// The current state of the block.
        /// </summary>
        public State State { get; set; }
    }

    /// <summary>
    /// Called when the height of the block should be modified.
    /// </summary>
    public IEvent<ModifyHeightMessage> ModifyHeight { get; private set; } = null!;
}
