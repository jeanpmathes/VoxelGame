// <copyright file="Wet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
///     Behavior of blocks that can be wet.
/// </summary>
public partial class Wet : BlockBehavior, IBehavior<Wet, BlockBehavior, Block>
{
    private Wet(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<BecomeWetMessage> BecomeWet { get; set; }

    /// <inheritdoc />
    public static Wet Construct(Block input)
    {
        return new Wet(input);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        BecomeWet = registry.RegisterEvent<BecomeWetMessage>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (!BecomeWet.HasSubscribers) return;

        Boolean wasWet = IsWet(message.OldState.Block) || message.OldState.Fluid.Fluid.IsLiquid;
        Boolean isWet = IsWet(message.NewState.Block) || message.NewState.Fluid.Fluid.IsLiquid;

        if (wasWet || !isWet) return;

        BecomeWetMessage becomeWet = IEventMessage<BecomeWetMessage>.Pool.Get();
            
        {
            becomeWet.World = message.World;
            becomeWet.Position = message.Position;
        }
            
        BecomeWet.Publish(becomeWet);
            
        IEventMessage<BecomeWetMessage>.Pool.Return(becomeWet);
    }

    /// <summary>
    ///     Get whether the block is wet based on its state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>True if the block is wet, false otherwise.</returns>
    public Boolean IsWet(State state)
    {
        return state.Fluid?.IsLiquid == true;
    }

    /// <summary>
    ///     Sent when a block becomes wet.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IBecomeWetMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the block is located.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; }
    }
}
