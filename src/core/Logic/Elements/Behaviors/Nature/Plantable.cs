// <copyright file="Plantable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// The behavior for blocks that can support plant growth.
/// </summary>
public partial class Plantable : BlockBehavior, IBehavior<Plantable, BlockBehavior, Block>
{
    private Plantable(Block subject) : base(subject)
    {
        SupportsFullGrowthInitializer = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(SupportsFullGrowthInitializer), this);
    }
    
    /// <summary>
    /// Whether this block supports full plant growth.
    /// This means that plants can reach all growth stages and are not limited to only the first few stages.
    /// </summary>
    public Boolean SupportsFullGrowth { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="SupportsFullGrowth"/> property.
    /// </summary>
    public Aspect<Boolean, Block> SupportsFullGrowthInitializer { get; }
    
    // todo: when visualizing aspects, maybe filter out by type of second argument
    // todo: so if it is Block then this are init-only aspects, all others are runtime aspects
    
    /// <inheritdoc/>
    public static Plantable Construct(Block input)
    {
        return new Plantable(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        SupportsFullGrowth = SupportsFullGrowthInitializer.GetValue(original: false, Subject);
    }

    /// <inheritdoc/>
    public override void DefineEvents(IEventRegistry registry)
    {
        GrowthAttempt = registry.RegisterEvent<GrowthAttemptMessage>(single: true);
    }
    
    /// <summary>
    ///     Try to grow a plant on this block.
    /// </summary>
    /// <param name="world">The world in which the operation takes place.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="fluid">The fluid that is required by the plant.</param>
    /// <param name="level">The amount of fluid required by the plant.</param>
    /// <returns>True if enough fluid was available.</returns>
    public Boolean TryGrow(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (!GrowthAttempt.HasSubscribers) 
            return fluid.TryTakeExact(world, position, level);

        GrowthAttemptMessage message = new(this)
        {
            World = world,
            Position = position,
            Fluid = fluid,
            Level = level
        };
            
        GrowthAttempt.Publish(message);

        return message.CanGrow;
    }
    
    /// <summary>
    /// Sent when a plant attempts to grow on this block.
    /// </summary>
    public record GrowthAttemptMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        /// The world in which the placement was completed.
        /// </summary>
        public World World { get; set; } = null!;
        
        /// <summary>
        /// The position at which the block was placed.
        /// </summary>
        public Vector3i Position { get; set; }
        
        /// <summary>
        /// The fluid that is required by the plant.
        /// </summary>
        public Fluid Fluid { get; set; } = null!;
        
        /// <summary>
        /// The amount of fluid required by the plant.
        /// </summary>
        public FluidLevel Level { get; set; }
        
        /// <summary>
        /// Whether the plant can grow on this block.
        /// If this is set to <c>true</c> by a subscriber, it must remove the fluid from the world.
        /// </summary>
        public Boolean CanGrow { get; set; } = false;
    }

    [LateInitialization]
    private partial IEvent<GrowthAttemptMessage> GrowthAttempt { get; set; }
}
