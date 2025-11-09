// <copyright file="Core.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     These blocks are the most essential blocks in the game.
///     The game relies on these blocks to exist and on their IDs to be fixed.
/// </summary>
public class Core(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     The air block that fills the world. Could also be interpreted as "no block".
    /// </summary>
    public Block Air { get; } = builder
        .BuildUnmeshedBlock(new CID(nameof(Air)), Language.Air)
        .WithBehavior<Static>()
        .WithBehavior<Fillable>()
        .WithProperties(flags => flags.IsSolid.ContributeConstant(value: false))
        .WithProperties(flags => flags.IsEmpty.ContributeConstant(value: true))
        .WithBehavior<Replaceable>()
        .WithValidation((block, validator) =>
        {
            if (block.BlockID != 0) 
                validator.ReportError($"Block {block} must have block ID 0");
        })
        .Complete();

    /// <summary>
    ///     An error block, used as fallback when structure operations fail.
    /// </summary>
    public Block Error { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Error)), Language.Error)
        .WithTextureLayout(TextureLayout.Uniform(TID.MissingTexture))
        .Complete();

    /// <summary>
    ///     The core of the world, which is found at the lowest level.
    /// </summary>
    public Block CoreBlock { get; } = builder
        .BuildSimpleBlock(new CID(nameof(CoreBlock)), Language.Core)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("core")))
        .Complete();

    /// <summary>
    ///     A block that serves as a neutral choice for development purposes.
    /// </summary>
    public Block Dev { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Dev)), Language.DevBlock)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("dev")))
        .Complete();
}
