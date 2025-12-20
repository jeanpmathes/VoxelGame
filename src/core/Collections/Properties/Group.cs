// <copyright file="Group.cs" company="VoxelGame">
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
using System.Collections;
using System.Collections.Generic;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A group combines multiple properties into a single unit.
/// </summary>
public class Group : Property, IEnumerable<Property>
{
    private readonly List<Property> children = [];

    /// <summary>
    ///     Creates a new group with the given name and children.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="children">The children of the group.</param>
    public Group(String name, IEnumerable<Property>? children = null) : base(name)
    {
        if (children != null)
            this.children.AddRange(children);
    }

    /// <inheritdoc />
    public IEnumerator<Property> GetEnumerator()
    {
        return children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Add a child to the group.
    /// </summary>
    /// <param name="property">The child to add.</param>
    public void Add(Property property)
    {
        children.Add(property);
    }

    internal sealed override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}
