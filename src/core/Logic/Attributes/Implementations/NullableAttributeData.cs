// <copyright file="NullableAttributeData.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Text.Json.Nodes;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal sealed class NullableAttributeData<TValue>(IAttributeData<TValue> valueAttributeData) : AttributeDataImplementation<TValue?> where TValue : struct
{
    public override Int32 Multiplicity { get; } = valueAttributeData.Multiplicity + 1;

    public override TValue? Retrieve(Int32 index)
    {
        return index == 0 ? null : valueAttributeData.Retrieve(index - 1);
    }

    public override Int32 Provide(TValue? value)
    {
        return value is {} inner ? valueAttributeData.Provide(inner) + 1 : 0;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return index == 0
            ? new Message(Name, "null")
            : new Group(Name, [valueAttributeData.RetrieveRepresentation(index - 1)]);
    }

    public override JsonNode GetValues(State state)
    {
        JsonObject obj = new();

        TValue? value = state.Get(this);
        obj["isNull"] = value is null;

        if (value is not null)
            obj["value"] = valueAttributeData.GetValues(state);

        return obj;
    }

    public override State SetValues(State state, JsonNode values)
    {
        if (values is not JsonObject obj) return state;

        if (obj["isNull"]?.GetValue<Boolean>() == true)
            return state.With(this, value: null);

        return obj["value"] is not null ? valueAttributeData.SetValues(state, obj["value"]!) : state;
    }
}
