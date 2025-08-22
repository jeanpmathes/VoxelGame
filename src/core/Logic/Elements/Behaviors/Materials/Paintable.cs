// <copyright file="Paintable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Materials;

/// <summary>
/// Blocks that can be painted with different colors.
/// </summary>
public class Paintable : BlockBehavior, IBehavior<Paintable, BlockBehavior, Block>
{
    private IAttribute<NamedColor> Color => color ?? throw Exceptions.NotInitialized(nameof(color));
    private IAttribute<NamedColor>? color;
    
    private Paintable(Block subject) : base(subject)
    {
        subject.Require<Meshed>().Tint.ContributeFunction(GetTint);
    }

    /// <inheritdoc/>
    public static Paintable Construct(Block input)
    {
        return new Paintable(input);
    }

    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        color = builder.Define(nameof(color)).Enum<NamedColor>().Attribute();
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ActorInteractionMessage>(OnActorInteract);
    }

    private ColorS GetTint(ColorS original, State state)
    {
        return state.Get(Color).ToColorS();
    }
    
    private void OnActorInteract(Block.ActorInteractionMessage message)
    {
        NamedColor currentColor = message.State.Get(Color);
        
        State newState = message.State.With(Color, (NamedColor) (((Int32) currentColor + 1) % (Int32) (NamedColor.Viridian + 1)));
        
        message.Actor.World.SetBlock(new BlockInstance(newState), message.Position);
    }
}
