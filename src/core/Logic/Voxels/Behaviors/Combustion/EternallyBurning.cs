// <copyright file="EternallyBurning.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;

/// <summary>
///     Does not stop burning.
/// </summary>
public partial class EternallyBurning : BlockBehavior, IBehavior<EternallyBurning, BlockBehavior, Block>
{
    [Constructible]
    private EternallyBurning(Block subject) : base(subject)
    {
        subject.Require<Combustible>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Combustible.IBurnMessage>(OnBurn);
    }

    private static void OnBurn(Combustible.IBurnMessage message)
    {
        // Nothing to do, subscription just prevents fall-back behavior.
    }
}
