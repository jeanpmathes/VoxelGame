// <copyright file="IScoped.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Text.Json.Nodes;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
///     Any attribute or scope that can be part of a <see cref="Scope" />.
/// </summary>
public interface IScoped
{
    /// <summary>
    ///     The name of the attribute or scope.
    ///     Must be unique within the current scope.
    /// </summary>
    String Name { get; }

    /// <summary>
    ///     Whether this scoped item is empty, meaning there are no attributes within it.
    ///     Attributes themselves are never empty.
    /// </summary>
    Boolean IsEmpty { get; }

    /// <summary>
    ///     Get the value or values of this scoped item for a given state.
    ///     Use this only for serialization or debugging purposes which are not performance critical.
    /// </summary>
    /// <param name="state">The state to get the value(s) for.</param>
    /// <returns>The value or values of the scoped item for the given state.</returns>
    JsonNode GetValues(State state);

    /// <summary>
    ///     Set the value or values of this scoped item for a given state.
    ///     Use this only for deserialization or debugging purposes which are not performance critical.
    /// </summary>
    /// <param name="state">The state to set the value(s) for.</param>
    /// <param name="values">The value or values to set. Incorrect types will be ignored.</param>
    /// <returns>The state with the updated value(s).</returns>
    State SetValues(State state, JsonNode values);

    /// <summary>
    ///     Get a property representation of this scoped item for a given state.
    /// </summary>
    /// <param name="state">The state to get the representation for.</param>
    /// <returns>The property representing the value(s) of the scoped item for the given state.</returns>
    Property GetRepresentation(State state);
}
