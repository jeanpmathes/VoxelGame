// <copyright file="Property.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     This is the base class for the elements of the property collection.
///     It uses the composite pattern to allow to freely combine data of different types.
/// </summary>
public abstract class Property
{
    /// <summary>
    ///     Creates a new property with the given name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    protected Property(String name)
    {
        Name = name;
    }

    /// <summary>
    ///     The name of the property.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     Accepts a visitor and calls the appropriate method.
    /// </summary>
    internal abstract void Accept(Visitor visitor);
}
