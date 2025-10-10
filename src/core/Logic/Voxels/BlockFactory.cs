// <copyright file="BlockFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Creates blocks based on a set of parameters.
/// </summary>
public class BlockFactory
{
    private readonly List<Block> blocksByID = [];
    private readonly Dictionary<String, Block> blocksByNamedID = [];
    
    private readonly HashSet<Block> blocksWithCollisionOnID = [];
    private Int32 idCollisionCounter;
 
    /// <summary>
    ///     Get a container associating block IDs to blocks.
    /// </summary>
    public IReadOnlyList<Block> BlocksByID => blocksByID;

    /// <summary>
    ///     Get a container associating block named IDs to blocks.
    /// </summary>
    public IReadOnlyDictionary<String, Block> BlocksByNamedID => blocksByNamedID;
    
    /// <summary>
    ///     Get a set of blocks that had a collision on their named ID during creation.
    /// </summary>
    public IReadOnlySet<Block> BlocksWithCollisionOnID => blocksWithCollisionOnID;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="namedID">The named ID of the block. A unique and unlocalized identifier.</param>
    /// <param name="name">The name of the block. Can be localized.</param>
    /// <param name="meshable">The type of meshing this block uses.</param>
    public Block Create(String namedID, String name, Meshable meshable)
    {
        var idCollision = false;
        
        if (blocksByNamedID.TryGetValue(namedID, out Block? collidedBlock))
        {
            Debugger.Break();
            idCollision = true;
            
            blocksWithCollisionOnID.Add(collidedBlock);
            
            namedID = $"{namedID}_collision_{idCollisionCounter++}";
        }
        
        Block block = CreateBlock(namedID, name, meshable);

        blocksByID.Add(block);
        blocksByNamedID.Add(namedID, block);
        
        if (idCollision) 
            blocksWithCollisionOnID.Add(block);

        return block;
    }

    private Block CreateBlock(String namedID, String name, Meshable meshable)
    {
        var id = (UInt32) blocksByID.Count;

        return meshable switch
        {
            Meshable.Simple => new SimpleBlock(id, namedID, name),
            Meshable.Foliage => new FoliageBlock(id, namedID, name),
            Meshable.Complex => new ComplexBlock(id, namedID, name),
            Meshable.PartialHeight => new PartialHeightBlock(id, namedID, name),
            Meshable.Unmeshed => new UnmeshedBlock(id, namedID, name),
            _ => throw Exceptions.UnsupportedEnumValue(meshable)
        };
    }
}
