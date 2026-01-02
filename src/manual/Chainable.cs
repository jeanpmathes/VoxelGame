// <copyright file="Chainable.cs" company="VoxelGame">
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
using VoxelGame.Manual.Elements;
using VoxelGame.Manual.Modifiers;
using Boolean = VoxelGame.Manual.Elements.Boolean;

namespace VoxelGame.Manual;

/// <summary>
///     Allows the simple creating of ordered elements.
/// </summary>
public abstract class Chainable
{
    internal abstract void AddElement(IElement element);

    /// <summary>
    ///     Add text content to the section.
    /// </summary>
    /// <param name="content">The text content to add.</param>
    /// <param name="style">The style of the text to add.</param>
    /// <returns>The current chain.</returns>
    public Chainable Text(String content, TextStyle style = TextStyle.Normal)
    {
        AddElement(new Text(content, style));

        return this;
    }

    /// <summary>
    ///     Add a key box to the section.
    /// </summary>
    /// <param name="k">The key to describe.</param>
    /// <returns>The current chain.</returns>
    public Chainable Key(Object k)
    {
        AddElement(new Key(k));

        return this;
    }

    /// <summary>
    ///     Start a new line.
    /// </summary>
    /// <returns>This.</returns>
    public Chainable NewLine()
    {
        AddElement(new NewLine());

        return this;
    }

    /// <summary>
    ///     Add a representation of a boolean value.
    /// </summary>
    /// <param name="value">The value to represent.</param>
    /// <returns>This.</returns>
    public Chainable Boolean(System.Boolean value)
    {
        AddElement(new Boolean(value));

        return this;
    }

    /// <summary>
    ///     Add a list to the section.
    /// </summary>
    /// <param name="builder">The builder for the list.</param>
    /// <returns>This.</returns>
    public Chainable List(Func<Chainable, Chainable> builder)
    {
        List list = new();
        AddElement(list);

        builder(list);

        return this;
    }

    /// <summary>
    ///     Add an item. Only makes sense in a list.
    /// </summary>
    /// <param name="bullet">An optional bullet leading the item.</param>
    /// <returns>This.</returns>
    public Chainable Item(String? bullet = null)
    {
        AddElement(new Item(bullet));

        return this;
    }

    /// <summary>
    ///     Add a subsection to this chainable.
    /// </summary>
    /// <param name="title">The title of the subsection.</param>
    /// <param name="builder">The builder for the subsection.</param>
    /// <returns>This.</returns>
    public Chainable SubSection(String title, Func<Chainable, Chainable> builder)
    {
        AddElement(Manual.SubSection.Create(title, builder));

        return this;
    }

    /// <summary>
    ///     Add a table to the section.
    /// </summary>
    /// <param name="columns">The column specification of the table.</param>
    /// <param name="builder">The builder for the table.</param>
    /// <returns>This.</returns>
    public Chainable Table(String columns, Action<Table> builder)
    {
        Table table = new(columns);
        AddElement(table);

        builder(table);

        return this;
    }
}
