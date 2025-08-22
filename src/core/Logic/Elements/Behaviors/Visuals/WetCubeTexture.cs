// <copyright file="WetCubeTexture.cs" company="VoxelGame">
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
/// Behavior that swaps out the texture of a block when it is wet.
/// Uses the <see cref="CubeTextured"/> behavior.
/// </summary>
public class WetCubeTexture : BlockBehavior, IBehavior<WetCubeTexture, BlockBehavior, Block>
{
    private WetCubeTexture(Block subject) : base(subject)
    {
        WetTextureInitializer = Aspect<TextureLayout, Block>.New<Exclusive<TextureLayout, Block>>(nameof(WetTextureInitializer), this);

        subject.Require<Wet>();
        subject.Require<CubeTextured>().ActiveTexture.ContributeFunction((original, state) => state.Fluid?.IsLiquid == true ? WetTexture : original);
    }
    
    /// <summary>
    /// The texture layout to use when the block is wet.
    /// </summary>
    public TextureLayout WetTexture { get; private set; } = TextureLayout.Uniform(TID.MissingTexture);
    
    /// <summary>
    /// Aspect used to initialize the <see cref="WetTexture"/> property.
    /// </summary>
    public Aspect<TextureLayout, Block> WetTextureInitializer { get; }

    /// <inheritdoc/>
    public static WetCubeTexture Construct(Block input)
    {
        return new WetCubeTexture(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        WetTexture = WetTextureInitializer.GetValue(TextureLayout.Uniform(TID.MissingTexture), Subject);
    }
}
