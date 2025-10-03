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
///     The behavior for blocks that can support plant growth.
/// </summary>
public partial class Plantable : BlockBehavior, IBehavior<Plantable, BlockBehavior, Block>
{
    private Plantable(Block subject) : base(subject)
    {
        SupportsFullGrowthInitializer = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(SupportsFullGrowthInitializer), this);
    }

    /// <summary>
    ///     Whether this block supports full plant growth.
    ///     This means that plants can reach all growth stages and are not limited to only the first few stages.
    /// </summary>
    public Boolean SupportsFullGrowth { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="SupportsFullGrowth" /> property.
    /// </summary>
    public Aspect<Boolean, Block> SupportsFullGrowthInitializer { get; }

    [LateInitialization] private partial IEvent<IGrowthAttemptMessage> GrowthAttempt { get; set; }

    // todo: when visualizing aspects, maybe filter out by type of second argument
    // todo: so if it is Block then this are init-only aspects, all others are runtime aspects

    /// <inheritdoc />
    public static Plantable Construct(Block input)
    {
        return new Plantable(input);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        GrowthAttempt = registry.RegisterEvent<IGrowthAttemptMessage>(single: true);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        SupportsFullGrowth = SupportsFullGrowthInitializer.GetValue(original: false, Subject);
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

        GrowthAttemptMessage growthAttempt = IEventMessage<GrowthAttemptMessage>.Pool.Get();
        
        {
            growthAttempt.World = world;
            growthAttempt.Position = position;
            growthAttempt.Fluid = fluid;
            growthAttempt.Level = level;
            growthAttempt.CanGrow = false;
        }
        
        GrowthAttempt.Publish(growthAttempt);
        
        Boolean canGrow = growthAttempt.CanGrow;
        
        IEventMessage<GrowthAttemptMessage>.Pool.Return(growthAttempt);
        
        return canGrow;
    }

    /// <summary>
    ///     Sent when a plant attempts to grow on this block.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IGrowthAttemptMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the placement was completed.
        /// </summary>
        public World World { get; } 

        /// <summary>
        ///     The position at which the block was placed.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The fluid that is required by the plant.
        /// </summary>
        public Fluid Fluid { get; }

        /// <summary>
        ///     The amount of fluid required by the plant.
        /// </summary>
        public FluidLevel Level { get; }

        /// <summary>
        ///     Whether the plant can grow on this block.
        /// </summary>
        public Boolean CanGrow { get; }
        
        /// <summary>
        /// Mark that the growth attempt succeeded.
        /// A caller calling this must remove the required fluid from the world.
        /// </summary>
        public void MarkAsSuccessful();
    }
    
    private partial record GrowthAttemptMessage
    {
        public void MarkAsSuccessful()
        {
            CanGrow = true;
        }
    }
}
