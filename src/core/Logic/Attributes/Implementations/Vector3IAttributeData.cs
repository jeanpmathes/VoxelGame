// <copyright file="Vector3IAttributeData.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming must match the type name.")]
internal sealed class Vector3IAttributeData(Vector3i max) : AttributeDataImplementation<Vector3i>
{
    private readonly Int32 maxXY = max.X * max.Y;

    public override Int32 Multiplicity { get; } = max.X * max.Y * max.Z;

    public override Vector3i Retrieve(Int32 index)
    {
        Int32 z = index / maxXY;
        Int32 y = index / max.X % max.Y;
        Int32 x = index % max.X;

        return new Vector3i(x, y, z);
    }

    public override Int32 Provide(Vector3i value)
    {
        Debug.Assert(value.X >= 0 && value.X < max.X);
        Debug.Assert(value.Y >= 0 && value.Y < max.Y);
        Debug.Assert(value.Z >= 0 && value.Z < max.Z);

        return value.Z * maxXY + value.Y * max.X + value.X;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Group(Name,
        [
            new Integer("X", Retrieve(index).X),
            new Integer("Y", Retrieve(index).Y),
            new Integer("Z", Retrieve(index).Z)
        ]);
    }

    public override JsonNode GetValues(State state)
    {
        Vector3i vector = state.Get(this);

        JsonObject obj = new()
        {
            ["X"] = vector.X,
            ["Y"] = vector.Y,
            ["Z"] = vector.Z
        };

        return obj;
    }

    public override State SetValues(State state, JsonNode values)
    {
        if (values is not JsonObject obj) return state;

        Int32 x = obj["X"]?.GetValue<Int32>() ?? 0;
        Int32 y = obj["Y"]?.GetValue<Int32>() ?? 0;
        Int32 z = obj["Z"]?.GetValue<Int32>() ?? 0;

        return state.With(this, new Vector3i(x, y, z));
    }
}
