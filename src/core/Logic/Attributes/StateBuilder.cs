// <copyright file="StateBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Attributes.Implementations;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Used to define the <see cref="StateSet"/> of a block by defining the attributes of a block.
/// </summary>
public class StateBuilder
{
    private readonly List<(IAttribute attribute, UInt64 divisor)> attributes = [];
    
    private UInt64 count = 1; 

    /// <summary>
    /// Defines a new attribute of type <see cref="Boolean"/> with the given name.
    /// </summary>
    /// <param name="name">The name of the attribute, must be unique within the scope.</param>
    /// <returns>The defined attribute.</returns>
    public IAttribute<Boolean> DefineBoolean(String name) // todo: scoping
    {
        return AddAttribute(new BooleanAttribute(name) { Index = attributes.Count });
    }
    
    /// <summary>
    /// Defines a new attribute of type <see cref="Int32"/> with the given name and range.
    /// </summary>
    /// <param name="name">The name of the attribute, must be unique within the scope.</param>
    /// <param name="min">The minimum value of the attribute, inclusive.</param>
    /// <param name="max">The maximum value of the attribute, exclusive.</param>
    /// <returns>The defined attribute.</returns>
    public IAttribute<Int32> DefineInt32(String name, Int32 min, Int32 max) // todo: scoping
    {
        return AddAttribute(new Int32Attribute(name, min, max) { Index = attributes.Count });
    }

    private IAttribute<TValue> AddAttribute<TValue>(IAttribute<TValue> attribute)
    {
        // todo: validate that less than Int32.MaxValue possible states per block
        
        attributes.Add((attribute, count));
        count *= attribute.Multiplicity;
        
        return attribute;
    }
    
    /// <summary>
    /// Builds the <see cref="StateSet"/> with the defined attributes.
    /// </summary>
    /// <param name="setOffset">The offset of the state set within the global state space.</param>
    /// <returns>The state set.</returns>
    public StateSet Build(UInt64 setOffset)
    {
        return new StateSet(
            setOffset: setOffset,
            attributes: attributes.ToArray()
        );
    }
}