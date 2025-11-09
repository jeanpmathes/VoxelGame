// <copyright file="BooleanAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Text.Json.Nodes;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class BooleanAttribute : AttributeImplementation<Boolean>
{
    public override Int32 Multiplicity => 2;

    public override Boolean Retrieve(Int32 index)
    {
        return index == 1;
    }

    public override Int32 Provide(Boolean value)
    {
        return value ? 1 : 0;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Truth(Name, Retrieve(index));
    }

    public override JsonNode GetValues(State state)
    {
        return state.Get(this);
    }

    public override State SetValues(State state, JsonNode values)
    {
        return state.With(this, values.GetValue<Boolean>());
    }
}
