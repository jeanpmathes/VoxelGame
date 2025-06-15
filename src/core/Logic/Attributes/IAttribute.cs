// <copyright file="IAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Base interface for attributes.
/// See <see cref="IAttribute{TValue}"/> for the more specific interface.
/// </summary>
public interface IAttribute
{
    /// <summary>
    /// The name of the attribute.
    /// </summary>
    public String Name { get; }

    /// <summary>
    /// The index of the attribute, assigned in order of definition.
    /// </summary>
    internal Int32 Index { get; init; }
    
    /// <summary>
    /// How many different values this attribute can take.
    /// </summary>
    public UInt64 Multiplicity { get; }
}

/// <summary>
/// An attribute is a value that depends on the block state.
/// Attributes assign an index to each possible value they can take.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IAttribute<out TValue> : IAttribute
{
    /// <summary>
    /// Gets the value of the attribute for a given index.
    /// </summary>
    /// <param name="index">The index, will be in the range [0, <see cref="IAttribute.Multiplicity"/>).</param>
    /// <returns>The value of the attribute for the given index.</returns>
    TValue Retrieve(Int32 index);

    /// <summary>
    /// Get the value of the attribute for a given <see cref="State"/>.
    /// </summary>
    /// <param name="state">The state to get the value for.</param>
    /// <returns>The value of the attribute for the given state.</returns>
    public TValue Get(State state)
    {
        Int32 index = state.GetValueIndex(this);
        
        return Retrieve(index);
    }
}
