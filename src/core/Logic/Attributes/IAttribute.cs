﻿// <copyright file="IAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Base interface for attributes.
/// See <see cref="IAttribute{TValue}"/> for the more specific interface.
/// </summary>
public interface IAttribute : IScoped
{
    /// <summary>
    ///     The divisor of the attribute, which is used to calculate the value index from the state ID.
    /// </summary>
    internal Int32 Divisor { get; }

    /// <summary>
    ///     How many different values this attribute can take.
    /// </summary>
    internal Int32 Multiplicity { get; }

    Property IScoped.GetRepresentation(State state)
    {
        Debug.Assert(Divisor != 0);

        return RetrieveRepresentation(GetValueIndex(state.Index));
    }

    /// <summary>
    /// Retrieve the representation of the attribute for a given index.
    /// </summary>
    /// <param name="index">The value index, which will be in the range [0, <see cref="Multiplicity"/>).</param>
    /// <returns>The property representing the value of the attribute for the given index.</returns>
    internal Property RetrieveRepresentation(Int32 index);

    /// <summary>
    /// Get the value index of this attribute in the given state.
    /// </summary>
    /// <param name="index">The state index to get the value index for.</param>
    /// <returns>The attribute value index for the given state, which will be in the range [0, <see cref="Multiplicity"/>).</returns>
    internal Int32 GetValueIndex(Int32 index)
    {
        return index / Divisor % Multiplicity;
    }

    /// <summary>
    /// Get the state index for a given value index.
    /// </summary>
    /// <param name="index">The value index, which will be in the range [0, <see cref="Multiplicity"/>).</param>
    /// <returns>The state index for the given value index.</returns>
    internal Int32 GetStateIndex(Int32 index)
    {
        return index * Divisor;
    }
}

/// <summary>
/// An attribute is a value that depends on the block state.
/// Attributes assign an index to each possible value they can take.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IAttribute<TValue> : IAttribute
{
    /// <summary>
    /// Gets the value of the attribute for a given index.
    /// </summary>
    /// <param name="index">The index, which will be in the range [0, <see cref="IAttribute.Multiplicity"/>).</param>
    /// <returns>The value of the attribute for the given index.</returns>
    TValue Retrieve(Int32 index);

    /// <summary>
    /// Get the value of the attribute for a given <see cref="State"/>.
    /// </summary>
    /// <param name="index">The state index of the state to get the value for.</param>
    /// <returns>The value of the attribute for the given state.</returns>
    internal TValue Get(Int32 index)
    {
        if (Divisor == 0) 
            return default!;
        
        return Multiplicity == 1 
            ? Retrieve(0) 
            : Retrieve(GetValueIndex(index));
    }

    /// <summary>
    ///     Provide the value index for a given value.
    /// </summary>
    /// <param name="value">The value to provide an index for.</param>
    /// <returns>The index of the value, which must be in the range [0, <see cref="IAttribute.Multiplicity" />).</returns>
    Int32 Provide(TValue value);

    /// <summary>
    ///     Set the value of the attribute within an otherwise zero state.
    /// </summary>
    /// <param name="value">The value to set for the attribute.</param>
    /// <returns>The state index for the new value.</returns>
    internal Int32 Set(TValue value)
    {
        if (Divisor == 0 || Multiplicity == 1)
            return 0;
        
        return GetStateIndex(Provide(value));
    }
}

/// <summary>
///     Abstract base class for attributes.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public abstract class AttributeImplementation<TValue> : IAttribute<TValue>
{
    /// <inheritdoc />
    public String Name { get; private set; } = null!;

    /// <inheritdoc />
    public Int32 Divisor { get; private set; }

    /// <inheritdoc />
    public abstract Int32 Multiplicity { get; }

    /// <inheritdoc />
    public abstract TValue Retrieve(Int32 index);

    /// <inheritdoc />
    public abstract Int32 Provide(TValue value);

    /// <inheritdoc />
    public abstract Property RetrieveRepresentation(Int32 index);

    internal void Initialize(String name, Int32 divisor)
    {
        Debug.Assert(Divisor == 0);
        Debug.Assert(divisor != 0);

        Name = name;

        Divisor = divisor;
    }
}
