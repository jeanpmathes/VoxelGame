// <copyright file = "Substance.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
/// How replaceable a block is.
/// </summary>
public enum Substance
{
    /// <summary>
    /// The block can be replaced by any other block without having to destroy it first.
    /// Additionally, the block can be considered as completely empty.
    /// Prefer this over checking for specific blocks like <see cref="Core.Air"/>.
    /// </summary>
    Empty,
    
    /// <summary>
    /// The block can be replaced by any block without having to destroy it first.
    /// </summary>
    Replaceable,
    
    /// <summary>
    /// The block cannot be replaced directly.
    /// Instead, one may only place another block at its position after successfully destroying it first.
    /// </summary>
    Normal
}
