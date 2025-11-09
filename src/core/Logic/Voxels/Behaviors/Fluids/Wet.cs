// <copyright file="Wet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Behavior of blocks that can be wet.
/// </summary>
public partial class Wet : BlockBehavior, IBehavior<Wet, BlockBehavior, Block>
{
    [Constructible]
    private Wet(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<IBecomeWetMessage> BecomeWet { get; set; }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        BecomeWet = registry.RegisterEvent<IBecomeWetMessage>();
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
            
        becomeWet.World = message.World;
        becomeWet.Position = message.Position;
            
        BecomeWet.Publish(becomeWet);
            
        IEventMessage<BecomeWetMessage>.Pool.Return(becomeWet);
    }

    /// <summary>
    ///     Get whether the block is wet based on its state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>True if the block is wet, false otherwise.</returns>
    private static Boolean IsWet(State state)
    {
        return state.Fluid?.IsLiquid == true;
    }

    /// <summary>
    ///     Sent when a block becomes wet.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IBecomeWetMessage
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
