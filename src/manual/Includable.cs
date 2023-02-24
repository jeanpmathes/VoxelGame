// <copyright file="Includable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    private readonly string name;
    private readonly FileInfo outputPath;

    private readonly List<Section> sections = new();

    /// <summary>
    ///     Create a new includable document.
    /// </summary>
    /// <param name="name">The name of this document.</param>
    /// <param name="outputPath">The output path to produce a file at.</param>
    public Includable(string name, DirectoryInfo outputPath)
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
    ///     Create a new section and add it to this document.
    /// </summary>
    /// <param name="title">The title of the section to add.</param>
    /// <returns>The created section.</returns>
    public Section CreateSection(string title)
    {
        var section = Section.Create(title);
        AddSection(section);

        return section;
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
