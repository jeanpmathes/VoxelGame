﻿// <copyright file="FontBundle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using Gwen.Net;
using Gwen.Net.Skin;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI.Utilities;

/// <summary>
///     A bundle of all fonts used by the GUI.
/// </summary>
public sealed class FontBundle : IResource
{
    private const String DefaultFontName = "Times New Roman";
    private const String ConsoleFontName = "Consolas";

    private readonly List<Font> headers = [];

    internal FontBundle(SkinBase skin)
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

        headers.Add(Title);
        headers.Add(Font.Create(skin.Renderer, DefaultFontName, size: 25));
        headers.Add(Font.Create(skin.Renderer, DefaultFontName, size: 20));
        headers.Add(Font.Create(skin.Renderer, DefaultFontName, size: 18, FontStyle.Bold));
        headers.Add(Font.Create(skin.Renderer, DefaultFontName, size: 16, FontStyle.Bold));
    }

    internal Font Default { get; }

    internal Font Title { get; }
    internal Font Subtitle { get; }
    internal Font Small { get; }
    internal Font Path { get; }
    internal Font PathU { get; }

    internal Font Console { get; }
    internal Font ConsoleError { get; }

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<FontBundle>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.FontBundle;

    /// <summary>
    ///     Get the header, using the one-based level.
    /// </summary>
    internal Font GetHeader(Int32 level)
    {
        return headers[Math.Clamp(level - 1, min: 0, headers.Count - 1)];
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        Title.Dispose();
        Subtitle.Dispose();
        Small.Dispose();
        Path.Dispose();
        PathU.Dispose();

        Console.Dispose();
        ConsoleError.Dispose();

        foreach (Font font in headers.Skip(count: 1)) font.Dispose();

        disposed = true;
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~FontBundle()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
