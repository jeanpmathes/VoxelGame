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

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// Creates blocks based on a set of parameters.
/// </summary>
public class BlockFactory
{
    private readonly List<Block> blocksByID = [];
    private readonly Dictionary<String, Block> blocksByNamedID = [];
    
    /// <summary>
    /// Get a container associating block IDs to blocks.
    /// </summary>
    public IReadOnlyList<Block> BlocksByID => blocksByID;
    
    /// <summary>
    /// Get a container associating block named IDs to blocks.
    /// </summary>
    public IReadOnlyDictionary<String, Block> BlocksByNamedID => blocksByNamedID;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="namedID">The named ID of the block. A unique and unlocalized identifier.</param>
    /// <param name="name">The name of the block. Can be localized.</param>
    /// <param name="meshable">The type of meshing this block uses.</param>
    public Block Create(String namedID, String name, Meshable meshable)
    {
        Block block = CreateBlock(namedID, name, meshable);

        if (blocksByNamedID.ContainsKey(namedID))
        {
            Debugger.Break();
            
            // todo: think about how to handle this
        }
        
        blocksByID.Add(block);
        blocksByNamedID.Add(namedID, block);
        
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
