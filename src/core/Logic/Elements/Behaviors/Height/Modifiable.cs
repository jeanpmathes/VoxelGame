// <copyright file="Modifiable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Height;

/// <summary>
///     Allows to modify the height by interacting with the block.
/// </summary>
public partial class Modifiable : BlockBehavior, IBehavior<Modifiable, BlockBehavior, Block>
{
    private Modifiable(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<ModifyHeightMessage> ModifyHeight { get; set; }

    /// <inheritdoc />
    public static Modifiable Construct(Block input)
    {
        return new Modifiable(input);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        ModifyHeight = registry.RegisterEvent<ModifyHeightMessage>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorInteractionMessage>(OnActorInteractions);
    }

    private void OnActorInteractions(Block.IActorInteractionMessage message)
    {
        if (!ModifyHeight.HasSubscribers) return;

        ModifyHeightMessage modifyHeight = IEventMessage<ModifyHeightMessage>.Pool.Get();

        {
            modifyHeight.World = message.Actor.World;
            modifyHeight.Position = message.Position;
            modifyHeight.State = message.State;
        }

        ModifyHeight.Publish(modifyHeight);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!ModifyHeight.HasSubscribers)
            validator.ReportWarning("Modifiable behavior has no effect as there are no subscribers");
    }

    /// <summary>
    ///     Sent when the height of the block should be modified.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IModifyHeightMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the block is located.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block in the world.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The current state of the block.
        /// </summary>
        public State State { get; }
    }
}
