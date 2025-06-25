// <copyright file="StateSet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Elements.New;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Defines the set of all states of a block.
/// </summary>
public class StateSet
{
    private readonly UInt64 setOffset;
    private readonly UInt64 generationDefault;
    private readonly IReadOnlyList<IScoped> entries;

    /// <summary>
    ///     Create a new state set for a block.
    /// </summary>
    /// <param name="block">The block that this state set belongs to.</param>
    /// <param name="setOffset">The offset of the state IDs in this set within the global state ID space.</param>
    /// <param name="stateCount">The number of states in this set.</param>
    /// <param name="generationDefault">The default state ID to use when the block is placed by world generation.</param>
    /// <param name="entries">The entries in this state set, which can be either attributes or nested scopes.</param>
    public StateSet(Block block, UInt64 setOffset, UInt64 stateCount, UInt64 generationDefault, IReadOnlyList<IScoped> entries)
    {
        this.setOffset = setOffset;
        this.entries = entries;
        this.generationDefault = generationDefault;

        Count = stateCount;
        Block = block;
    }

    /// <summary>
    /// The number of states in the set.
    /// </summary>
    public UInt64 Count { get; }

    /// <summary>
    ///     The block that this state set belongs to.
    /// </summary>
    public Block Block { get; }

    /// <summary>
    ///     Get the entries of this state set.
    /// </summary>
    public IEnumerable<IScoped> Entries => entries;

    /// <summary>
    ///     Get the default state of this set.
    ///     The default state is not necessarily the best state to place blocks in.
    /// </summary>
    public State Default => new(this, Index: 0);

    /// <summary>
    ///     Get the default state for world generation.
    /// </summary>
    public State GenerationDefault => new(this, generationDefault);

    /// <summary>
    /// Get the state for a given state ID.
    /// </summary>
    /// <param name="id">The state ID, which is a number across all blocks.</param>
    /// <returns>The state corresponding to the ID.</returns>
    public State GetState(UInt64 id)
    {
        // todo: when calling, do not forget to mask out fluid info 

        return new State(this, id - setOffset);
    }
}
