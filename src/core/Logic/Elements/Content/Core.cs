// <copyright file="Core.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// These blocks are the most essential blocks in the game.
/// The game relies on these blocks to exist and on their IDs to be fixed.
/// </summary>
public class Core(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     The air block that fills the world. Could also be interpreted as "no block".
    /// </summary>
    public Block Air { get; } = builder
        .BuildUnmeshedBlock(Language.Air, nameof(Air))
        .WithBehavior<Static>()
        .WithBehavior<Fillable>()
        .WithProperties(flags => flags.IsSolid.ContributeConstant(value: false))
        .WithProperties(flags => flags.IsReplaceable.ContributeConstant(value: true))
        // todo: add validation that this block receives ID 0 and the only state has state ID 0
        .Complete();
    
    /// <summary>
    ///     An error block, used as fallback when structure operations fail.
    /// </summary>
    public Block Error { get; } = builder
        .BuildSimpleBlock(Language.Error, nameof(Error))
        .WithTextureLayout(TextureLayout.Uniform(TID.MissingTexture))
        .Complete();
    
    /// <summary>
    ///     The core of the world, which is found at the lowest level.
    /// </summary>
    public Block CoreBlock { get; } = builder
        .BuildSimpleBlock(Language.Core, nameof(CoreBlock))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("core")))
        .Complete();
    
    /// <summary>
    ///     A block that serves as a neutral choice for development purposes.
    /// </summary>
    public Block Dev { get; } = builder
        .BuildSimpleBlock(Language.DevBlock, nameof(Dev))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("dev")))
        .Complete();
}
