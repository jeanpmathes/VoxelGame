// <copyright file="Text.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.IO;
using VoxelGame.Manual.Modifiers;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A simple text element.
/// </summary>
internal class Text : IElement
{
    private readonly TextStyle style;

    internal Text(string text, TextStyle style)
    {
        Content = text;
        this.style = style;
    }

    private string Content { get; }

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
                throw new NotSupportedException();
        }
    }
}
