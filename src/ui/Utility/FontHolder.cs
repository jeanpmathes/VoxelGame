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

    internal FontHolder(SkinBase skin)
    {
        skin.DefaultFont = new Font(skin.Renderer, DefaultFontName, size: 15);
        Default = skin.DefaultFont;

        Title = Font.Create(skin.Renderer, DefaultFontName, size: 30);
        Subtitle = Font.Create(skin.Renderer, DefaultFontName);
        Small = Font.Create(skin.Renderer, DefaultFontName, size: 12);

        Path = Font.Create(skin.Renderer, ConsoleFontName);
        PathU = Font.Create(skin.Renderer, ConsoleFontName, style: FontStyle.Underline);

        Console = Font.Create(skin.Renderer, ConsoleFontName, size: 15);
        ConsoleError = Font.Create(skin.Renderer, ConsoleFontName, size: 15, FontStyle.Bold);
    }

    internal Font Default { get; }

    internal Font Title { get; }
    internal Font Subtitle { get; }
    internal Font Small { get; }
    internal Font Path { get; }
    internal Font PathU { get; }

    internal Font Console { get; }
    internal Font ConsoleError { get; }

    #region IDisposable Support

    private bool disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        Title.Dispose();
        Subtitle.Dispose();
        Small.Dispose();
        Path.Dispose();

        Console.Dispose();
        ConsoleError.Dispose();

        disposed = true;
    }

    ~FontHolder()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
