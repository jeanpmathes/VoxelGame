// <copyright file="FontHolder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Skin;

namespace VoxelGame.UI.Utility;

/// <summary>
///     Holds all fonts used by the user interface.
/// </summary>
internal sealed class FontHolder : IDisposable
{
    private const string DefaultFontName = "Times New Roman";
    private const string ConsoleFontName = "Consolas";

    private readonly SkinBase skin;

    internal FontHolder(SkinBase skin)
    {
        this.skin = skin;
        skin.DefaultFont = new Font(skin.Renderer, DefaultFontName, size: 15);

        Title = Font.Create(skin.Renderer, DefaultFontName, size: 30);
        Subtitle = Font.Create(skin.Renderer, DefaultFontName);
        Small = Font.Create(skin.Renderer, DefaultFontName, size: 12);
        Path = Font.Create(skin.Renderer, DefaultFontName, style: FontStyle.Italic);

        Console = Font.Create(skin.Renderer, ConsoleFontName, size: 15);
        ConsoleError = Font.Create(skin.Renderer, ConsoleFontName, size: 15, FontStyle.Bold);
    }

    internal Font Default => skin.DefaultFont;

    internal Font Title { get; }
    internal Font Subtitle { get; }
    internal Font Small { get; }
    internal Font Path { get; }

    internal Font Console { get; }
    internal Font ConsoleError { get; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        skin.Dispose();
        Title.Dispose();
        Subtitle.Dispose();
        Small.Dispose();
        Path.Dispose();
    }

    ~FontHolder()
    {
        Dispose(disposing: false);
    }
}

