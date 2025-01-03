﻿// <copyright file="List.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A list of elements.
/// </summary>
internal class List : Chainable, IElement
{
    private readonly List<IElement> elements = [];

    void IElement.Generate(StreamWriter writer)
    {
        writer.WriteLine(@"\begin{itemize}[nosep]");

        foreach (IElement element in elements) element.Generate(writer);

        writer.WriteLine(@"\end{itemize}");
    }

    internal override void AddElement(IElement element)
    {
        elements.Add(element);
    }
}
