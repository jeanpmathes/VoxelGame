﻿// <copyright file="GrowingPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;

/// <summary>
/// An extension of a <see cref="Plant"/> which grows over time.
/// </summary>
public class GrowingPlant : BlockBehavior, IBehavior<GrowingPlant, BlockBehavior, Block>
{
    private IAttribute<Int32?> Stage => stage ?? throw Exceptions.NotInitialized(nameof(stage));
    private IAttribute<Int32?>? stage;
    
    /// <summary>
    /// The number of growth stages this plant has.
    /// </summary>
    public Int32 StageCount { get; private set; } = 1;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="StageCount"/> property.
    /// </summary>
    public Aspect<Int32, Block> StageCountInitializer { get; }
    
    /// <summary>
    /// Whether the plant can grow in the current state.
    /// </summary>
    public Aspect<Boolean, State> CanGrow { get; }
    
    private Int32 MatureStage => StageCount - 1;

    private GrowingPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();
        
        StageCountInitializer = Aspect<Int32, Block>.New<Exclusive<Int32, Block>>(nameof(StageCountInitializer), this);
        
        CanGrow = Aspect<Boolean, State>.New<ANDing<State>>(nameof(CanGrow), this);
    }
    
    /// <inheritdoc />
    public static GrowingPlant Construct(Block input)
    {
        return new GrowingPlant(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        StageCount = StageCountInitializer.GetValue(original: 1, Subject);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        stage = builder.Define(nameof(stage)).Int32(min: 0, StageCount).NullableAttribute(placementDefault: 0, generationDefault: 0);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        MatureUpdate = registry.RegisterEvent<MatureUpdateMessage>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.RandomUpdateMessage>(OnRandomUpdate);
    }
    
    private void OnRandomUpdate(Block.RandomUpdateMessage message)
    {
        if (!CanGrow.GetValue(original: true, message.State))
            return;
        
        Int32? currentStage = message.State.Get(Stage);
        
        if (currentStage is not {} aliveStage)
            return;

        BlockInstance? below = message.World.GetBlock(message.Position.Below());
        
        if (below?.Block.Get<Plantable>() is not {} plantable)
            return;
        
        if (aliveStage == MatureStage)
        {
            MatureUpdate.Publish(new MatureUpdateMessage(this)
            {
                World = message.World,
                Position = message.Position,
                State = message.State,
                Ground = plantable
            });
            
            return;
        }
        
        State newState = message.State;

        if (currentStage > 2) // todo: use aspect for this, might need to be lower for double crop plants, must be lowered for fruit plant
        {
            FluidInstance? fluid = message.World.GetFluid(message.Position.Below());
            
            if (fluid?.Fluid == Elements.Fluids.Instance.SeaWater) // todo: generalize this in some way
            {
                newState.Set(Stage, value: null);
            }
            else
            {
                if (!plantable.SupportsFullGrowth) return;
                if (!plantable.TryGrow(message.World, message.Position.Below(), Elements.Fluids.Instance.FreshWater, FluidLevel.One)) return;
                
                newState.Set(Stage, aliveStage + 1);
            }
        }
        else
        {
            newState.Set(Stage, aliveStage + 1);
        }
        
        message.World.SetBlock(new BlockInstance(newState), message.Position);
    }

    /// <summary>
    /// Sent when the plant receives a random update and has already reached the last stage.
    /// </summary>
    public record MatureUpdateMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        /// The world in which the plant is located.
        /// </summary>
        public World World { get; set; } = null!;
        
        /// <summary>
        /// The position of the plant in the world.
        /// </summary>
        public Vector3i Position { get; set; }
        
        /// <summary>
        /// The state of the plant block.
        /// </summary>
        public State State { get; set; }

        /// <summary>
        /// The plantable ground this plant is growing on.
        /// </summary>
        public Plantable Ground { get; set; } = null!;
    }
    
    /// <summary>
    /// Called when the plant receives a random update and has already reached the last stage.
    /// </summary>
    public IEvent<MatureUpdateMessage> MatureUpdate { get; private set; } = null!;
    
    /// <summary>
    /// Get the current growth stage of the plant from the given state.
    /// </summary>
    /// <param name="state">The state of the block to get the growth stage for.</param>
    /// <returns>The current growth stage, which is either a number between <c>0</c> and <see cref="StageCount"/>, or <c>null</c> if the plant died.</returns>
    public Int32? GetStage(State state)
    {
        return state.Get(Stage);
    }
}
