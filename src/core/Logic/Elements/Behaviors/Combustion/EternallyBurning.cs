// <copyright file="EternallyBurning.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Combustion;

/// <summary>
///     Does not stop burning.
/// </summary>
public class EternallyBurning : BlockBehavior, IBehavior<EternallyBurning, BlockBehavior, Block>
{
    private EternallyBurning(Block subject) : base(subject)
    {
        subject.Require<Combustible>();
    }

    /// <inheritdoc />
    public static EternallyBurning Construct(Block input)
    {
        return new EternallyBurning(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Combustible.BurnMessage>(OnBurn);
    }

    private static void OnBurn(Combustible.BurnMessage message)
    {
        message.Burned = false;
    }
}
