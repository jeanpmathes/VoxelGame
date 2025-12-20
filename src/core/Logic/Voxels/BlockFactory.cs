// <copyright file="BlockFactory.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Creates blocks based on a set of parameters.
/// </summary>
public class BlockFactory
{
    private readonly List<Block> blocksByBlockID = [];
    private readonly Dictionary<CID, Block> blocksByContentID = [];

    private readonly HashSet<Block> blocksWithCollisionOnID = [];
    private Int32 idCollisionCounter;

    /// <summary>
    ///     Get a container associating block IDs to blocks.
    /// </summary>
    public IReadOnlyList<Block> BlocksByBlockID => blocksByBlockID;

    /// <summary>
    ///     Get a container associating block content IDs to blocks.
    /// </summary>
    public IReadOnlyDictionary<CID, Block> BlocksByContentID => blocksByContentID;

    /// <summary>
    ///     Get a set of blocks that had a collision on their named ID during creation.
    /// </summary>
    public IReadOnlySet<Block> BlocksWithCollisionOnID => blocksWithCollisionOnID;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="contentID">The content ID of the block. Must be unique.</param>
    /// <param name="name">The name of the block. Can be localized.</param>
    /// <param name="meshable">The type of meshing this block uses.</param>
    public Block Create(CID contentID, String name, Meshable meshable)
    {
        var idCollision = false;

        if (blocksByContentID.TryGetValue(contentID, out Block? collidedBlock))
        {
            Debugger.Break();
            idCollision = true;

            blocksWithCollisionOnID.Add(collidedBlock);

            contentID = new CID($"{contentID}_collision_{idCollisionCounter++}");
        }

        Block block = CreateBlock(contentID, name, meshable);

        blocksByBlockID.Add(block);
        blocksByContentID.Add(contentID, block);

        if (idCollision)
            blocksWithCollisionOnID.Add(block);

        return block;
    }

    private Block CreateBlock(CID contentID, String name, Meshable meshable)
    {
        var id = (UInt32) blocksByBlockID.Count;

        return meshable switch
        {
            Meshable.Simple => new SimpleBlock(id, contentID, name),
            Meshable.Foliage => new FoliageBlock(id, contentID, name),
            Meshable.Complex => new ComplexBlock(id, contentID, name),
            Meshable.PartialHeight => new PartialHeightBlock(id, contentID, name),
            Meshable.Unmeshed => new UnmeshedBlock(id, contentID, name),
            _ => throw Exceptions.UnsupportedEnumValue(meshable)
        };
    }
}
