// <copyright file="Paintable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Blocks that can be painted with different colors.
/// </summary>
public partial class Paintable : BlockBehavior, IBehavior<Paintable, BlockBehavior, Block>
{
    [Constructible]
    private Paintable(Block subject) : base(subject)
    {
        subject.Require<Meshed>().Tint.ContributeFunction(GetTint);
    }

    [LateInitialization] private partial IAttribute<ColorS> Color { get; set; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorInteractionMessage>(OnActorInteract);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Color = builder.Define(nameof(Color)).List(ColorS.NamedColors, ColorS.GetNameOfNamedColorByIndex).Attribute();
    }

    private ColorS GetTint(ColorS original, State state)
    {
        return state.Get(Color);
    }

    private void OnActorInteract(Block.IActorInteractionMessage message)
    {
        Int32 currentIndex = message.State.GetValueIndex(Color);
        Int32 nextIndex = (currentIndex + 1) % ColorS.NamedColors.Count;

        State newState = message.State.With(Color, ColorS.GetNamedColorByIndex(nextIndex));

        message.Actor.World.SetBlock(newState, message.Position);
    }
}
