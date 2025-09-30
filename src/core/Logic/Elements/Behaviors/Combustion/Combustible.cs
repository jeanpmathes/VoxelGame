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

        BurnMessage message = new(this)
        {
            World = world,
            Position = position,
            Fire = fire,
            Burned = false
        };

        Burn.Publish(message);

        return message.Burned;
    }

    /// <summary>
    ///     Sent when a block is burned.
    /// </summary>
    public record BurnMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world the block is in.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position of the block that is burning.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The fire block that caused the burning.
        /// </summary>
        public Block Fire { get; set; } = null!;

        /// <summary>
        ///     Whether the block has been destroyed by the burn operation.
        ///     Subscribers can set this.
        /// </summary>
        public Boolean Burned { get; set; }
    }
}
