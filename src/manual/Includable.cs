// <copyright file="Includable.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;

namespace VoxelGame.Manual;

/// <summary>
///     A document that can be included in the manual, at defined insertion points.
/// </summary>
public class Includable
{
    private readonly String name;
    private readonly FileInfo outputPath;

    private readonly List<Section> sections = new();

    /// <summary>
    ///     Create a new includable document.
    /// </summary>
    /// <param name="name">The name of this document.</param>
    /// <param name="outputPath">The output path to produce a file at.</param>
    public Includable(String name, DirectoryInfo outputPath)
    {
        this.name = name;
        this.outputPath = outputPath.GetFile($"{name}.tex");
    }

    /// <summary>
    ///     Add a new section to this document.
    /// </summary>
    /// <param name="section">The section to add to the bottom.</param>
    public void AddSection(Section section)
    {
        sections.Add(section);
    }

    /// <summary>
    ///     Create and add multiple sections for a sequence of items.
    /// </summary>
    /// <param name="items">The items to add sections for.</param>
    /// <param name="generator">A generator creating sections for items.</param>
    /// <typeparam name="T">The item type.</typeparam>
    public void CreateSections<T>(IEnumerable<T> items, Func<T, Section> generator)
    {
        foreach (T item in items) AddSection(generator(item));
    }

    /// <summary>
    ///     Generate the output file.
    /// </summary>
    public void Generate()
    {
        using StreamWriter writer = outputPath.CreateText();

        foreach (Section section in sections)
        {
            section.Generate(writer, name);
            writer.WriteLine();
        }
    }
}
