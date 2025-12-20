// <copyright file="ListAttributeData.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class ListAttributeData<TElement>(IEnumerable<TElement> elements, Func<Int32, String>? representation) : AttributeDataImplementation<TElement> where TElement : struct
{
    private readonly TElement[] elements = [..elements];

    public override Int32 Multiplicity => elements.Length;

    public override TElement Retrieve(Int32 index)
    {
        return elements[index];
    }

    [PerformanceSensitive]
    public override Int32 Provide(TElement value)
    {
        for (var index = 0; index < elements.Length; index++)
            if (EqualityComparer<TElement>.Default.Equals(elements[index], value))
                return index;

        return 0;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Message(Name, $"[{index}] = {(representation != null ? representation(index) : elements[index].ToString())}");
    }

    public override JsonNode GetValues(State state)
    {
        return state.Get(this).ToString() ?? "";
    }

    [PerformanceSensitive]
    public override State SetValues(State state, JsonNode values)
    {
        foreach (TElement element in elements)
            if (element.ToString() == values.GetValue<String>())
                return state.With(this, element);

        return state;
    }
}
