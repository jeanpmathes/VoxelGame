// <copyright file="Combustible.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Combustion;

/// <summary>
///     Makes a block able to be burned.
/// </summary>
public partial class Combustible : BlockBehavior, IBehavior<Combustible, BlockBehavior, Block>
{
    private Combustible(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<BurnMessage> Burn { get; set; }

    /// <inheritdoc />
    public static Combustible Construct(Block input)
    {
        return new Combustible(input);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        Burn = registry.RegisterEvent<BurnMessage>();
    }

    /// <summary>
    ///     Burn a block at a given position.
    ///     The block can either be destroyed, or change into a different state or block.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="fire">The fire block that caused the burning.</param>
    /// <returns><c>true</c> if the block was destroyed, <c>false</c> if not.</returns>
    public Boolean DoBurn(World world, Vector3i position, Block fire)
    {
        if (!Burn.HasSubscribers)
            return Subject.Destroy(world, position);

        BurnMessage burn = IEventMessage<BurnMessage>.Pool.Get();

        {
            burn.World = world;
            burn.Position = position;
            burn.Fire = fire;
            burn.Burned = false;
        }

        Burn.Publish(burn);

        return burn.Burned;
    }
    
    /// <summary>
    ///     Sent when a block is burned.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IBurnMessage : IEventMessage
    {
        /// <summary>
        ///     The world the block is in.
        /// </summary>
        public World World { get; }
        
        /// <summary>
        ///     The position of the block that is burning.
        /// </summary>
        public Vector3i Position { get; }
        
        /// <summary>
        ///     The fire block that caused the burning.
        /// </summary>
        public Block Fire { get; }
        
        /// <summary>
        ///     Whether the block has been destroyed by the burn operation.
        /// </summary>
        public Boolean Burned { get; }

        /// <summary>
        /// Set that the block has been burned (destroyed or changed).
        /// This will set <see cref="Burned"/> to <c>true</c>.
        /// </summary>
        public void Burn();
    }
    
    private partial record BurnMessage
    {
        public void Burn()
        {
            Burned = true;
        }
    }
}
