// <copyright file="SubSection.cs" company="VoxelGame">
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
///     A subsection within a section.
/// </summary>
internal class SubSection(String sectionTitle) : Chainable, IElement
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
