// <copyright file="Int32AttributeData.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Text.Json.Nodes;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal sealed class Int32AttributeData(Int32 min, Int32 max) : AttributeDataImplementation<Int32>
{
    public override Int32 Multiplicity { get; } = max - min;

    public override Int32 Retrieve(Int32 index)
    {
        return min + index;
    }

    public override Int32 Provide(Int32 value)
    {
        Debug.Assert(value >= min && value < max);

        return value - min;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Integer(Name, Retrieve(index));
    }

    public override JsonNode GetValues(State state)
    {
        return state.Get(this);
    }

    public override State SetValues(State state, JsonNode values)
    {
        return state.With(this, values.GetValue<Int32>());
    }
}
