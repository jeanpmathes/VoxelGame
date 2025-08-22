﻿// <copyright file="State.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Refers to the state of a block.
/// </summary>
/// <param name="Owner">The set of all states of the block.</param>
/// <param name="Index">The index of the state in the set of the owning block.</param>
public record struct State(StateSet Owner, Int32 Index)
{
    /// <summary>
    /// Get the state ID of this state.
    /// It identifies this state uniquely across all states of all blocks.
    /// </summary>
    public UInt32 ID => StateSet.GetStateID(this);
    
    /// <summary>
    /// Get the block that this state belongs to.
    /// </summary>
    public Block Block => Owner.Block;

    /// <summary>
    /// Ugly fix to pretend that some states can have a fluid associated with them.
    /// </summary>
    public Fluid? Fluid => Owner.Block.Has<Fillable>() && Index % 2 == 0 ? Fluids.Instance.FreshWater : null;

    /// <summary>
    ///     Get the properties of this state, which are the attributes and their values for this state.
    /// </summary>
    /// <returns>The properties of this state, containing the attributes and their values.</returns>
    public IEnumerable<Property> CreateProperties()
    {
        foreach (IScoped entry in Owner.Entries) 
            yield return entry.GetRepresentation(this);
    }

    /// <summary>
    ///     Get the value of the given attribute for this state.
    /// </summary>
    /// <param name="attribute">The attribute to get the value for.</param>
    /// <typeparam name="TValue">The value type of the attribute.</typeparam>
    /// <returns>The value of the attribute for this state.</returns>
    public TValue Get<TValue>(IAttribute<TValue> attribute)
    {
        return attribute.Get(Index);
    }

    /// <summary>
    /// Set the value of the given attribute for this state.
    /// This will modify this state.
    /// </summary>
    /// <param name="attribute">The attribute to set the value for.</param>
    /// <param name="value">The value to set for the attribute.</param>
    /// <typeparam name="TValue">The value type of the attribute.</typeparam>
    public void Set<TValue>(IAttribute<TValue> attribute, TValue value) // todo: go through all State::Set and check if With would be better
    {
        if (attribute.Multiplicity == 1)
            return;
        
        Int32 oldIndex = Index;
        
        Int32 oldStateIndex = attribute.GetStateIndex(attribute.GetValueIndex(Index));
        oldIndex -= oldStateIndex;

        Int32 newStateIndex = attribute.Set(value);
        oldIndex += newStateIndex;
        
        Index = oldIndex;
    }
    
    /// <summary>
    /// Create a new state with the given attribute set to the given value.
    /// </summary>
    /// <param name="attribute">The attribute to set the value for.</param>
    /// <param name="value">The value to set for the attribute.</param>
    /// <typeparam name="TValue">The value type of the attribute.</typeparam>
    /// <returns>A new state with the attribute set to the given value.</returns>
    public State With<TValue>(IAttribute<TValue> attribute, TValue value) 
    {
        State newState = this;
        newState.Set(attribute, value);
        return newState;
    }
}
