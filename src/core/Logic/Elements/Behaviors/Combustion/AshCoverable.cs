// <copyright file="AshCoverable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Combustion;

/// <summary>
///     A block that is not <see cref="Combustible" /> but can be covered with ash if the block above it is burned.
/// </summary>
public partial class AshCoverable : BlockBehavior, IBehavior<AshCoverable, BlockBehavior, Block>
{
    private AshCoverable(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<IAshCoverMessage> AshCover { get; set; }

    /// <inheritdoc />
    public static AshCoverable Construct(Block input)
    {
        return new AshCoverable(input);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        AshCover = registry.RegisterEvent<IAshCoverMessage>();
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!AshCover.HasSubscribers)
            validator.ReportWarning("No operation registered to cover the block with ash");

        if (Subject.Is<Combustible>())
            validator.ReportWarning("Combustible blocks should not be coverable with ash as they burn instead");
    }

    /// <summary>
    ///     Cover the block with ash.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block to cover with ash.</param>
    public void CoverWithAsh(World world, Vector3i position)
    {
        if (!AshCover.HasSubscribers) return;

        AshCoverMessage ashCover = IEventMessage<AshCoverMessage>.Pool.Get();

        {
            ashCover.World = world;
            ashCover.Position = position;
        }

        AshCover.Publish(ashCover);
        
        IEventMessage<AshCoverMessage>.Pool.Return(ashCover);
    }

    /// <summary>
    ///     Sent when a block should be covered with ash.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IAshCoverMessage : IEventMessage
    {
        /// <summary>
        ///     The world the block is in.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block to cover with ash.
        /// </summary>
        public Vector3i Position { get; }
    }
}
