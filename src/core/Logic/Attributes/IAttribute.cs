// <copyright file="IAttribute.cs" company="VoxelGame">
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
    internal UInt64 Divisor { get; }

    /// <summary>
    ///     How many different values this attribute can take.
    /// </summary>
    internal UInt64 Multiplicity { get; }

    Property IScoped.GetRepresentation(State state)
    {
        Debug.Assert(Divisor == 0);

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
    internal Int32 GetValueIndex(UInt64 index)
    {
        return (Int32) (index / Divisor % Multiplicity);
    }

    /// <summary>
    /// Get the state index for a given value index.
    /// </summary>
    /// <param name="index">The value index, which will be in the range [0, <see cref="Multiplicity"/>).</param>
    /// <returns>The state index for the given value index.</returns>
    internal UInt64 GetStateIndex(Int32 index)
    {
        return (UInt64) index * Divisor;
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
    internal TValue Get(UInt64 index)
    {
        return Divisor == 0 ? default! : Retrieve(GetValueIndex(index));
    }

    /// <summary>
    ///     Provide the value index for a given value.
    /// </summary>
    /// <param name="value">The value to provide an index for.</param>
    /// <returns>The index of the value, which must be in the range [0, <see cref="IAttribute.Multiplicity" />).</returns>
    Int32 Provide(TValue value);

    /// <summary>
    ///     Set the value of the attribute for a given <see cref="State" />.
    ///     The passed state will not be modified.
    /// </summary>
    /// <param name="value">The value to set for the attribute.</param>
    /// <returns>The state index for the new value.</returns>
    internal UInt64 Set(TValue value)
    {
        return GetStateIndex(Provide(value));
    }
}

/// <summary>
///     Abstract base class for attributes.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public abstract class Attribute<TValue> : IAttribute<TValue>
{
    /// <summary>
    ///     The description of the attribute, which is used for documentation.
    /// </summary>
    public String Description { get; private set; } = null!;

    /// <inheritdoc />
    public String Name { get; private set; } = null!;

    /// <inheritdoc />
    public UInt64 Divisor { get; private set; }

    /// <inheritdoc />
    public abstract UInt64 Multiplicity { get; }

    /// <inheritdoc />
    public abstract TValue Retrieve(Int32 index);

    /// <inheritdoc />
    public abstract Int32 Provide(TValue value);

    /// <inheritdoc />
    public abstract Property RetrieveRepresentation(Int32 index);

    internal void Initialize(String name, String? description, UInt64 divisor)
    {
        Debug.Assert(Divisor == 0);
        Debug.Assert(divisor != 0);

        Name = name;
        Description = description ?? String.Empty;

        Divisor = divisor;
    }
}
