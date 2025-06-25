// <copyright file="Scope.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
///     A scope in which attributes are defined.
///     Attributes must be named uniquely within a scope.
/// </summary>
/// <param name="Name">The name of the scope.</param>
/// <param name="Entries">The entries in the scope, which can be either attributes or nested scopes.</param>
public record Scope(String Name, IReadOnlyList<IScoped> Entries) : IScoped
{
    /// <inheritdoc />
    public Property GetRepresentation(State state)
    {
        List<Property> properties = [];

        foreach (IScoped entry in Entries) properties.Add(entry.GetRepresentation(state));

        return new Group(Name, properties);
    }
}
