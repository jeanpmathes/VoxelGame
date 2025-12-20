// <copyright file="Section.cs" company="VoxelGame">
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
using System.IO;
using VoxelGame.Manual.Elements;

namespace VoxelGame.Manual;

/// <summary>
///     A section of a document, describing a topic.
/// </summary>
public class Section : Chainable
{
    private readonly List<IElement> elements = [];
    private readonly String title;

    private Section(String title)
    {
        this.title = title;
    }

    /// <summary>
    ///     Create a new section.
    /// </summary>
    /// <param name="title">The title of the section.</param>
    /// <param name="builder">The builder for the section.</param>
    /// <returns>The created section.</returns>
    public static Section Create(String title, Func<Chainable, Chainable> builder)
    {
        Section section = new(title);

        builder(section);

        return section;
    }

    internal override void AddElement(IElement element)
    {
        elements.Add(element);
    }

    internal void Generate(StreamWriter writer, String parent)
    {
        var id = $"{parent.ToLowerInvariant()}_{title.ToLowerInvariant()}";

        writer.WriteLine(@$"\subsection{{{title}}}\label{{subsec:{id}}}");

        foreach (IElement element in elements)
            if (element is SubSection subsection)
                subsection.Generate(writer, id);
            else
                element.Generate(writer);
    }
}
