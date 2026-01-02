// <copyright file="FlagsAttributeData.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Text.Json.Nodes;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal sealed class FlagsAttributeData<TFlags> : AttributeDataImplementation<TFlags> where TFlags : struct, Enum
{
    public override Int32 Multiplicity { get; } = 1 << EnumTools.CountFlags<TFlags>();

    public override TFlags Retrieve(Int32 index)
    {
        return EnumTools.GetEnumValue<TFlags>((UInt64) index);
    }

    public override Int32 Provide(TFlags value)
    {
        return (Int32) EnumTools.GetUnsignedValue(value);
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        TFlags value = Retrieve(index);

        List<Property> flags = new(EnumTools.CountFlags<TFlags>());

        foreach ((String name, TFlags flag) in EnumTools.GetPositions<TFlags>()) flags.Add(new Truth(name, value.HasFlag(flag)));

        return new Group(Name, flags);
    }

    public override JsonNode GetValues(State state)
    {
        return state.Get(this).ToString();
    }

    public override State SetValues(State state, JsonNode values)
    {
        return Enum.TryParse(values.GetValue<String>(), out TFlags result) ? state.With(this, result) : state;
    }
}
