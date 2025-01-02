// <copyright file="Text.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using VoxelGame.Manual.Modifiers;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A simple text element.
/// </summary>
internal class Text : IElement
{
    private readonly TextStyle style;

    internal Text(String text, TextStyle style)
    {
        Content = text;
        this.style = style;
    }

    private String Content { get; }

    void IElement.Generate(StreamWriter writer)
    {
        switch (style)
        {
            case TextStyle.Monospace:
                writer.WriteLine($@"\texttt{{{Content}}}");

                break;

            case TextStyle.Normal:
                writer.WriteLine(Content);

                break;

            default:
                throw Exceptions.UnsupportedEnumValue(style);
        }
    }
}
