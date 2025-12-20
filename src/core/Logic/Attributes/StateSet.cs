// <copyright file="StateSet.cs" company="VoxelGame">
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
using System.Text.Json.Nodes;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
///     Defines the set of all states of a block.
/// </summary>
public class StateSet
{
    private readonly IReadOnlyList<IScoped> entries;
    private readonly Int32 generationDefault;
    private readonly Int32 placementDefault;
    private readonly UInt32 setOffset;

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
    ///     The number of states in the set.
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
    ///     Get all states in this set.
    ///     Note that this will generate the states on the fly, so do not use this after loading.
    ///     The order of states is guaranteed to be the same as the order of state indices.
    /// </summary>
    public IEnumerable<State> AllStates
    {
        get
        {
            for (var index = 0; index < Count; index++) yield return GetStateByIndex(index);
        }
    }

    /// <summary>
    ///     Get all states in this set along with their indices.
    ///     Note that this will generate the states on the fly, so do not use this after
    ///     The order of states is guaranteed to be the same as the order of state indices.
    /// </summary>
    /// <returns>An enumerable of all states in this set along with their indices.</returns>
    public IEnumerable<(State, Int32)> AllStatesWithIndex
    {
        get
        {
            for (var index = 0; index < Count; index++) yield return (GetStateByIndex(index), index);
        }
    }

    /// <summary>
    ///     Get the state for a given state ID.
    /// </summary>
    /// <param name="id">The state ID, which is a number across all blocks.</param>
    /// <returns>The state corresponding to the ID.</returns>
    public State GetStateByID(UInt32 id)
    {
        return new State(this, (Int32) (id - setOffset));
    }

    /// <summary>
    ///     Get the state for a given index.
    /// </summary>
    /// <param name="index">The state index, which is a number greater or equal to 0 and less than <see cref="Count" />.</param>
    /// <returns>The state corresponding to the index.</returns>
    public State GetStateByIndex(Int32 index)
    {
        Debug.Assert(index >= 0 && index < Count);

        return new State(this, index);
    }

    /// <summary>
    ///     Get the state ID for a given state.
    /// </summary>
    /// <param name="state">The state to get the ID for.</param>
    /// <returns>The state ID, which is a number across all blocks.</returns>
    public static UInt32 GetStateID(State state)
    {
        return state.Owner.setOffset + (UInt32) state.Index;
    }

    /// <summary>
    ///     Convert a state to a JSON node.
    ///     This should be used for debugging or serialization, not for regular in-game use.
    /// </summary>
    /// <returns>The created dictionary.</returns>
    public JsonNode GetJson(State state)
    {
        JsonObject node = new();

        foreach (IScoped entry in entries)
        {
            if (entry.IsEmpty) continue;

            node[entry.Name] = entry.GetValues(state);
        }

        return node;
    }

    /// <summary>
    ///     Get the state from a JSON node.
    ///     Unknown elements will be ignored.
    /// </summary>
    /// <param name="node">The JSON node to read from. Must be a dictionary.</param>
    /// <returns>The created state.</returns>
    public State SetJson(JsonNode node)
    {
        State state = Default;

        if (node is not JsonObject obj) return state;

        foreach (IScoped entry in entries)
        {
            if (entry.IsEmpty) continue;

            if (!obj.TryGetPropertyValue(entry.Name, out JsonNode? inner))
                continue;

            state = entry.SetValues(state, inner!);
        }

        return state;
    }
}
