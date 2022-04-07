// <copyright file="FontHolder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Gwen.Net;
using Gwen.Net.Skin;

namespace VoxelGame.UI.Utility;

/// <summary>
///     Holds all fonts used by the user interface.
/// </summary>
internal sealed class FontHolder : IDisposable
{
    private const string FontName = "Arial";

    private readonly SkinBase skin;

    internal FontHolder(SkinBase skin)
    {
        this.skin = skin;
        skin.DefaultFont.Size = 15;

        Title = Font.Create(skin.Renderer, FontName, size: 30);
        Subtitle = Font.Create(skin.Renderer, FontName);
        Small = Font.Create(skin.Renderer, FontName, size: 12);
        Path = Font.Create(skin.Renderer, FontName, size: 10, FontStyle.Italic);
    }

    internal Font Default => skin.DefaultFont;

    internal Font Title { get; }
    internal Font Subtitle { get; }
    internal Font Small { get; }
    internal Font Path { get; }

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
