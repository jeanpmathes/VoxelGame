// <copyright file="WetTint.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
///     Behavior that applies a wet tint to a block when it is wet.
/// </summary>
public class WetTint : BlockBehavior, IBehavior<WetTint, BlockBehavior, Block>
{
    private WetTint(Block subject) : base(subject)
    {
        WetColorInitializer = Aspect<ColorS, Block>.New<Exclusive<ColorS, Block>>(nameof(WetColorInitializer), this);

        subject.Require<Wet>();
        subject.Require<Meshed>().Tint.ContributeFunction((original, state) => state.Fluid?.IsLiquid == true ? WetColor : original);
    }

    /// <summary>
    ///     The color tint to apply when the block is wet.
    /// </summary>
    public ColorS WetColor { get; private set; } = ColorS.LightGray;

    /// <summary>
    ///     Aspect used to initialize the <see cref="WetColor" /> property.
    /// </summary>
    public Aspect<ColorS, Block> WetColorInitializer { get; }

    /// <inheritdoc />
    public static WetTint Construct(Block input)
    {
        return new WetTint(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        WetColor = WetColorInitializer.GetValue(ColorS.LightGray, Subject);
    }
}
