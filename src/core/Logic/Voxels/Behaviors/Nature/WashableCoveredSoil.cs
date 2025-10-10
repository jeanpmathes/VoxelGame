// <copyright file="WashableCoveredSoil.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Extends <see cref="CoveredSoil" /> to remove the cover when becoming wet.
/// </summary>
public class WashableCoveredSoil : BlockBehavior, IBehavior<WashableCoveredSoil, BlockBehavior, Block>
{
    private readonly CoveredSoil soil;

    private WashableCoveredSoil(Block subject) : base(subject)
    {
        subject.Require<Wet>();

        soil = subject.Require<CoveredSoil>();
    }

    /// <inheritdoc />
    public static WashableCoveredSoil Construct(Block input)
    {
        return new WashableCoveredSoil(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Wet.IBecomeWetMessage>(OnBecomeWet);
    }

    private void OnBecomeWet(Wet.IBecomeWetMessage message)
    {
        soil.RemoveCover(message.World, message.Position);
    }
}
