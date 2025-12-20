// <copyright file="Visitor.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A visitor for the property collection.
/// </summary>
public class Visitor
{
    /// <summary>
    ///     Creates a new visitor.
    /// </summary>
    protected Visitor() {}

    /// <exclude />
    public void Visit(Property property)
    {
        property.Accept(this);
    }

    /// <exclude />
    public virtual void Visit(Group group)
    {
        foreach (Property child in group) Visit(child);
    }

    /// <exclude />
    public virtual void Visit(Error error)
    {
        // Nothing to do here.
    }

    /// <exclude />
    public virtual void Visit(Message message)
    {
        // Nothing to do here.
    }

    /// <exclude />
    public virtual void Visit(Integer integer)
    {
        // Nothing to do here.
    }

    /// <exclude />
    public virtual void Visit(FileSystemPath path)
    {
        // Nothing to do here.
    }

    /// <exclude />
    public virtual void Visit(Measure measure)
    {
        // Nothing to do here.
    }

    /// <exclude />
    public virtual void Visit(Truth truth)
    {
        // Nothing to do here.
    }

    /// <exclude />
    public virtual void Visit(Color color)
    {
        // Nothing to do here.
    }
}
