// <copyright file="Color.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A property that represents a color value.
/// </summary>
public class Color : Property
{
    /// <summary>
    ///     Creates a <see cref="Color" /> with a name and a color value.
    /// </summary>
    /// <param name="name">The name of the color property.</param>
    /// <param name="value">The color value of the color property.</param>
    public Color(String name, ColorS value) : base(name)
    {
        Value = value;
    }

    /// <summary>
    ///     Get the color value of the color property.
    /// </summary>
    public ColorS Value { get; }

    /// <exclude />
    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}
