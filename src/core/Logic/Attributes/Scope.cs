// <copyright file="Scope.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
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
    public Boolean IsEmpty
    {
        get
        {
            foreach (IScoped scoped in Entries)
                if (!scoped.IsEmpty)
                    return false;

            return true;
        }
    }

    /// <inheritdoc />
    public JsonNode GetValues(State state)
    {
        JsonObject values = new();

        foreach (IScoped entry in Entries) values[entry.Name] = entry.GetValues(state);

        return values;
    }

    /// <inheritdoc />
    public State SetValues(State state, JsonNode values)
    {
        if (values is not JsonObject obj) return state;

        foreach (IScoped entry in Entries)
        {
            if (!obj.TryGetPropertyValue(entry.Name, out JsonNode? value))
                continue;

            try
            {
                state = entry.SetValues(state, value!);
            }
            catch (InvalidOperationException)
            {
                // Ignore.
            }
        }

        return state;
    }

    /// <inheritdoc />
    public Property GetRepresentation(State state)
    {
        List<Property> properties = [];

        foreach (IScoped entry in Entries) properties.Add(entry.GetRepresentation(state));

        return new Group(Name, properties);
    }
}
