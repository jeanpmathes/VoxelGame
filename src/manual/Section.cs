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
    private readonly List<IElement> elements = new();
    private readonly String title;

    private Section(String title)
    {
        this.title = title;
    }

    /// <summary>
    ///     Create a new section.
    /// </summary>
    /// <param name="title">The title of the section.</param>
    /// <returns>The created section.</returns>
    public static Section Create(String title)
    {
        return new Section(title);
    }

    internal override void AddElement(IElement element)
    {
        elements.Add(element);
    }

    /// <inheritdoc />
    public override Section EndSection()
    {
        return this;
    }

    internal void Generate(StreamWriter writer, String parent)
    {
        writer.WriteLine(
            @$"\subsection{{{title}}}\label{{subsec:{parent.ToLowerInvariant()}_{title.ToLowerInvariant()}}}");

        foreach (IElement element in elements) element.Generate(writer);
    }
}
