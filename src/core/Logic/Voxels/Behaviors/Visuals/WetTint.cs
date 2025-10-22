// <copyright file="WetTint.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Behavior that applies a wet tint to a block when it is wet.
/// </summary>
public partial class WetTint : BlockBehavior, IBehavior<WetTint, BlockBehavior, Block>
{
    [Constructible]
    private WetTint(Block subject) : base(subject)
    {
        subject.Require<Wet>();
        subject.Require<Meshed>().Tint.ContributeFunction((original, state) => state.Fluid?.IsLiquid == true ? WetColor.Get() : original);
    }

    /// <summary>
    ///     The color tint to apply when the block is wet.
    /// </summary>
    public ResolvedProperty<ColorS> WetColor { get; } = ResolvedProperty<ColorS>.New<Exclusive<ColorS, Void>>(nameof(WetColor), ColorS.LightGray);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        WetColor.Initialize(this);
    }
}
