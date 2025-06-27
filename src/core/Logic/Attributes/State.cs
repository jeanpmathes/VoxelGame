// <copyright file="State.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Refers to the state of a block.
/// </summary>
/// <param name="Owner">The set of all states of the block.</param>
/// <param name="Index">The index of the state in the set.</param>
public record struct State(StateSet Owner, UInt64 Index)
{
    /// <summary>
    ///     Get the properties of this state, which are the attributes and their values for this state.
    /// </summary>
    /// <returns>The properties of this state, containing the attributes and their values.</returns>
    public Property CreateProperties()
    {
        List<Property> attributes = [];

        foreach (IScoped entry in Owner.Entries) attributes.Add(entry.GetRepresentation(this));

        return new Group($"State {Index} of {Owner.Block}", attributes);
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
    public void Set<TValue>(IAttribute<TValue> attribute, TValue value)
    {
        UInt64 oldIndex = attribute.GetStateIndex(attribute.GetValueIndex(Index));
        Index -= oldIndex;

        UInt64 newIndex = attribute.Set(value);
        Index += newIndex;
    }
}
