// <copyright file="IScoped.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    public String Name { get; }

    /// <summary>
    ///     Get a property representation of this scoped item for a given state.
    /// </summary>
    /// <param name="state">The state to get the representation for.</param>
    /// <returns>The property representing the value(s) of the scoped item for the given state.</returns>
    public Property GetRepresentation(State state);
}
