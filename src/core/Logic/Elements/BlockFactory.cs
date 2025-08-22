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
    
    public IReadOnlyList<Block> BlocksByID => blocksByID;
    public IReadOnlyDictionary<String, Block> BlocksByNamedID => blocksByNamedID;
    
    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="name">The name of the block. Can be localized.</param>
    /// <param name="namedID">The named ID of the block. A unique and unlocalized identifier.</param>
    /// <param name="meshable">The type of meshing this block uses.</param>
    public Block Create(String name, String namedID, Meshable meshable)
    {
        Block block = CreateBlock(name, namedID, meshable);

        if (blocksByNamedID.ContainsKey(namedID))
        {
            Debugger.Break();
            
            // todo: think about how to handle this
        }
        
        blocksByID.Add(block);
        blocksByNamedID.Add(namedID, block);
        
        return block;
    }

    private Block CreateBlock(String name, String namedID, Meshable meshable)
    {
        var id = (UInt32) blocksByID.Count;
        
        return meshable switch
        {
            Meshable.Simple => new SimpleBlock(name, id, namedID),
            Meshable.Foliage => new FoliageBlock(name, id, namedID),
            Meshable.Complex => new ComplexBlock(name, id, namedID),
            Meshable.PartialHeight => new PartialHeightBlock(name, id, namedID),
            Meshable.Unmeshed => new UnmeshedBlock(name, id, namedID),
            _ => throw Exceptions.UnsupportedEnumValue(meshable)
        };
    }
}
