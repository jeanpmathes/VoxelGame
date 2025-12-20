// <copyright file="SubSection.cs" company="VoxelGame">
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
///     A subsection within a section.
/// </summary>
internal sealed class SubSection(String sectionTitle) : Chainable, IElement
{
    private readonly List<IElement> elements = [];

    void IElement.Generate(StreamWriter writer)
    {
        Generate(writer, String.Empty);
    }

    internal static SubSection Create(String title, Func<Chainable, Chainable> builder)
    {
        SubSection subSection = new(title);

        builder(subSection);

        return subSection;
    }

    internal override void AddElement(IElement element)
    {
        elements.Add(element);
    }

    internal void Generate(StreamWriter writer, String parent)
    {
        writer.WriteLine(@$"\subsubsection{{{sectionTitle}}}\label{{subsubsec:{parent}_{sectionTitle.ToLowerInvariant()}}}");

        foreach (IElement element in elements) element.Generate(writer);
    }
}
