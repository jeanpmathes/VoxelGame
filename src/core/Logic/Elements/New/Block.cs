// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.New; // todo: move up in namespace

/// <summary>
/// The basic unit of the game world - a block.
/// Blocks use the flyweight pattern, the world data only stores a state ID.
/// The state ID can be used to retrieve both the type of the block and its state.
/// </summary>
public class Block
{
    /// <summary>
    /// The states of the block.
    /// </summary>
    public StateSet States { get; private set; } = null!;

    /// <summary>
    /// Initialize the block with its states.
    /// </summary>
    /// <param name="offset">The number of already existing block states.</param>
    /// <returns>The number of states of this block.</returns>
    public UInt64 Initialize(UInt64 offset)
    {
        StateBuilder builder = new();
        
        // todo: call all behaviors, do scoping
        
        States = builder.Build(offset);
        
        return States.Count;
    }
}