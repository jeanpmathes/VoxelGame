// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
        writer.WriteLine(
            @$"\subsection{{{title}}}\label{{subsec:{parent.ToLowerInvariant()}_{title.ToLowerInvariant()}}}");

        foreach (IElement element in elements) element.Generate(writer);
    }
}
