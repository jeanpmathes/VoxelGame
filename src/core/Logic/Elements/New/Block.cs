// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.New;
// todo: move up in namespace

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
    /// <param name="context">The context in which the block is initialized.</param>
    /// <returns>The number of states this block has.</returns>
    public UInt64 Initialize(UInt64 offset, IResourceContext context)
    {
        StateBuilder builder = new(context);

        foreach (String behavior in new[] {"x", "y"}) // todo: call all behaviors
            builder.Enclose(behavior, _ => {});

        States = builder.Build(this, offset);

        return States.Count;
    }
}
