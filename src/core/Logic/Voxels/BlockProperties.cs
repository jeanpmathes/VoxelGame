// <copyright file="BlockProperties.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Used to determine basic boolean aspects of a block.
/// </summary>
public class BlockProperties(Block subject)
{
    /// <summary>
    ///     Whether it is possible to see through this block.
    ///     Note that this only indicates whether the actual filled portion of the block is opaque.
    ///     If the block is not full, it is possible to see around the block.
    /// </summary>
    public Aspect<Boolean, Block> IsOpaque { get; } = Aspect<Boolean, Block>.New<LogicalAnd<Block>>(nameof(IsOpaque), subject);

    /// <summary>
    ///     This aspect is only relevant for non-opaque full blocks. It decides if their faces should be meshed next to
    ///     another non-opaque block.
    /// </summary>
    public Aspect<Boolean, Block> MeshFaceAtNonOpaques { get; } = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(MeshFaceAtNonOpaques), subject);

    /// <summary>
    ///     Whether this block hinders movement.
    /// </summary>
    public Aspect<Boolean, Block> IsSolid { get; } = Aspect<Boolean, Block>.New<LogicalAnd<Block>>(nameof(IsSolid), subject);
    
    /// <summary>
    ///     Gets whether this block is unshaded, which means it does not receive shadows.
    /// </summary>
    public Aspect<Boolean, Block> IsUnshaded { get; } = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(IsUnshaded), subject);
    
    /// <summary>
    ///     Whether this block is considered empty.
    /// </summary>
    public Aspect<Boolean, Block> IsEmpty { get; } = Aspect<Boolean, Block>.New<Exclusive<Boolean, Block>>(nameof(IsEmpty), subject);

}
