// <copyright file="AshCoverable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Combustion;

/// <summary>
/// A block that is not <see cref="Combustible"/> but can be covered with ash if the block above it is burned.
/// </summary>
public class AshCoverable : BlockBehavior, IBehavior<AshCoverable, BlockBehavior, Block>
{
    private AshCoverable(Block subject) : base(subject) {}
    
    /// <inheritdoc/>
    public static AshCoverable Construct(Block input)
    {
        return new AshCoverable(input);
    }

    /// <inheritdoc/>
    public override void DefineEvents(IEventRegistry registry)
    {
        AshCover = registry.RegisterEvent<AshCoverMessage>();
    }
    
    /// <inheritdoc/>
    protected override void OnValidate(IValidator validator)
    {
        if (!AshCover.HasSubscribers) 
            validator.ReportWarning("No operation registered to cover the block with ash");
        
        if (Subject.Is<Combustible>())
            validator.ReportWarning("Combustible blocks should not be coverable with ash as they burn instead");
    }

    /// <summary>
    /// Sent when a block should be covered with ash.
    /// </summary>
    public record AshCoverMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        /// The world the block is in.
        /// </summary>
        public World World { get; set; } = null!;
        
        /// <summary>
        /// The position of the block to cover with ash.
        /// </summary>
        public Vector3i Position { get; set; }
    }

    /// <summary>
    /// Called when the block should be covered with ash.
    /// </summary>
    public IEvent<AshCoverMessage> AshCover { get; private set; } = null!;
    
    /// <summary>
    /// Cover the block with ash.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block to cover with ash.</param>
    public void CoverWithAsh(World world, Vector3i position)
    {
        if (!AshCover.HasSubscribers) return;
        
        AshCoverMessage message = new(this)
        {
            World = world,
            Position = position
        };
        
        AshCover.Publish(message);
    }
}
