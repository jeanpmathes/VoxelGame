// <copyright file="StateSet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Defines the set of all states of a block.
/// </summary>
public class StateSet
{
    private readonly UInt64 setOffset;
    private readonly (IAttribute attribute, UInt64 divisor)[] attributes;
    
    public StateSet(UInt64 setOffset, (IAttribute attribute, UInt64 divisor)[] attributes)
    {
        this.setOffset = setOffset;
        this.attributes = attributes;
        
        Count = attributes[^1].divisor;
    }
    
    /// <summary>
    /// The number of states in the set.
    /// </summary>
    public UInt64 Count { get; }
    
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
    
    /// <summary>
    /// Given the index of a state and an attribute, get the value index of that attribute in the state.
    /// </summary>
    /// <param name="attribute">The attribute to get the index for.</param>
    /// <param name="index">The state index.</param>
    /// <returns>The attribute value index for the given state.</returns>
    public Int32 GetValueIndex(IAttribute attribute, UInt64 index)
    {
        UInt64 divisor = attributes[attribute.Index].divisor;
        UInt64 multiplicity = attribute.Multiplicity;
        UInt64 value = index / divisor % multiplicity;
        
        return (Int32) value;
    }
}