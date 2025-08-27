// <copyright file="StateSet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Defines the set of all states of a block.
/// </summary>
public class StateSet
{
    private readonly UInt32 setOffset;
    private readonly Int32 placementDefault;
    private readonly Int32 generationDefault; // todo: use this in world generation
    private readonly IReadOnlyList<IScoped> entries;

    /// <summary>
    ///     Create a new state set for a block.
    /// </summary>
    /// <param name="block">The block that this state set belongs to.</param>
    /// <param name="setOffset">The offset of the state IDs in this set within the global state ID space.</param>
    /// <param name="stateCount">The number of states in this set.</param>
    /// <param name="placementDefault">The default state ID to use when the block is placed by the player.</param>
    /// <param name="generationDefault">The default state ID to use when the block is placed by world generation.</param>
    /// <param name="entries">The entries in this state set, which can be either attributes or nested scopes.</param>
    public StateSet(Block block, UInt32 setOffset, UInt32 stateCount, Int32 placementDefault, Int32 generationDefault, IReadOnlyList<IScoped> entries)
    {
        Debug.Assert(stateCount <= Int32.MaxValue);
        
        this.setOffset = setOffset;
        this.entries = entries;
        this.placementDefault = placementDefault;
        this.generationDefault = generationDefault;

        Count = stateCount;
        Block = block;
    }

    /// <summary>
    /// The number of states in the set.
    /// </summary>
    public UInt32 Count { get; }

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
    ///     Get the default state for placement.
    /// </summary>
    public State PlacementDefault => new(this, placementDefault);

    /// <summary>
    ///     Get the default state for world generation.
    /// </summary>
    public State GenerationDefault => new(this, generationDefault);

    /// <summary>
    /// Get the state for a given state ID.
    /// </summary>
    /// <param name="id">The state ID, which is a number across all blocks.</param>
    /// <returns>The state corresponding to the ID.</returns>
    public State GetStateByID(UInt32 id)
    {
        return new State(this, (Int32)(id - setOffset));
    }
    
    /// <summary>
    /// Get the state for a given index.
    /// </summary>
    /// <param name="index">The state index, which is a number greater or equal to 0 and less than <see cref="Count"/>.</param>
    /// <returns>The state corresponding to the index.</returns>
    public State GetStateByIndex(Int32 index)
    {
        Debug.Assert(index >= 0 && index < Count);
        
        return new State(this, index);
    }
    
    /// <summary>
    /// Get the state ID for a given state.
    /// </summary>
    /// <param name="state">The state to get the ID for.</param>
    /// <returns>The state ID, which is a number across all blocks.</returns>
    public static UInt32 GetStateID(State state)
    {
        return state.Owner.setOffset + (UInt32) state.Index;
    }

    /// <summary>
    /// Get all states in this set.
    /// Note that this will generate the states on the fly, so do not use this after loading.
    /// The order of states is guaranteed to be the same as the order of state indices.
    /// </summary>
    /// <returns>An enumerable of all states in this set.</returns>
    public IEnumerable<State> GetAllStates()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return GetStateByIndex(index);
        }
    }
    
    /// <summary>
    /// Get all states in this set along with their indices.
    /// Note that this will generate the states on the fly, so do not use this after
    /// The order of states is guaranteed to be the same as the order of state indices.
    /// </summary>
    /// <returns>An enumerable of all states in this set along with their indices.</returns>
    public IEnumerable<(State, Int32)> GetAllStatesWithIndex()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return (GetStateByIndex(index), index);
        }
    }
}
