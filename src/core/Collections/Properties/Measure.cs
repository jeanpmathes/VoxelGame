// <copyright file="Measure.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A measure property, which contains a value that is associated with a unit.
/// </summary>
public class Measure : Property
{
    /// <summary>
    ///     Create a new measure property.
    /// </summary>
    public Measure(String name, IMeasure value) : base(name)
    {
        Value = value;
    }

    /// <summary>
    ///     The value of the measure.
    /// </summary>
    public IMeasure Value { get; set; }

    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}
