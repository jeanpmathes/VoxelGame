// <copyright file="WetCubeTexture.cs" company="VoxelGame">
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
///     Behavior that swaps out the texture of a block when it is wet.
///     Uses the <see cref="CubeTextured" /> behavior.
/// </summary>
public partial class WetCubeTexture : BlockBehavior, IBehavior<WetCubeTexture, BlockBehavior, Block>
{
    [Constructible]
    private WetCubeTexture(Block subject) : base(subject)
    {
        subject.Require<Wet>();
        subject.Require<CubeTextured>().ActiveTexture.ContributeFunction((original, state) => state.Fluid?.IsLiquid == true ? WetTexture.Get() : original);
    }

    /// <summary>
    ///     The texture layout to use when the block is wet.
    /// </summary>
    public ResolvedProperty<TextureLayout> WetTexture { get; } = ResolvedProperty<TextureLayout>.New<Exclusive<TextureLayout, Void>>(nameof(WetTexture), TextureLayout.Uniform(TID.MissingTexture));

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        WetTexture.Initialize(this);
    }
}
