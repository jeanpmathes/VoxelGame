// <copyright file="TextCache.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using Gwen.Net;
using Font = Gwen.Net.Font;
using Point = Gwen.Net.Point;
using Size = Gwen.Net.Size;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Stores text renderers and their associated text for reuse.
/// </summary>
public sealed class TextCache : IDisposable
{
    private readonly DirectXRenderer renderer;

    private Dictionary<(string, Font), Entry> strings = new();

    /// <summary>
    ///     Creates a new text cache.
    /// </summary>
    public TextCache(DirectXRenderer renderer)
    {
        StringFormat = new StringFormat(StringFormat.GenericTypographic);
        StringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

        this.renderer = renderer;
    }

    /// <summary>
    ///     Get the current size of the cache.
    /// </summary>
    public int Size => strings.Count;

    /// <summary>
    ///     Get the string format used by the cache.
    /// </summary>
    public StringFormat StringFormat { get; }

    /// <summary>
    ///     Dispose the cache, disposing all renderers.
    /// </summary>
    public void Dispose()
    {
        Flush();
        StringFormat.Dispose();
    }

    /// <summary>
    ///     Get the texture for the given text and font if it exists, otherwise return null.
    /// </summary>
    public Texture? GetTexture(Font font, string text)
    {
        if (!strings.TryGetValue((text, font), out Entry? entry)) return null;

        entry.Accessed = true;

        return entry.Renderer.Texture;
    }

    /// <summary>
    ///     Get or create the texture for the given text and font.
    /// </summary>
    public Texture GetOrCreateTexture(Font font, string text)
    {
        if (strings.TryGetValue((text, font), out Entry? entry))
        {
            entry.Accessed = true;

            return entry.Renderer.Texture;
        }

        Size size = renderer.MeasureText(font, text);
        entry = new Entry(new TextRenderer(size.Width, size.Height, renderer));
        entry.Renderer.SetString(text, (System.Drawing.Font) font.RendererData, Brushes.White, Point.Zero, StringFormat);

        strings[(text, font)] = entry;

        return entry.Renderer.Texture;
    }

    /// <summary>
    ///     Go trough the cache and remove all renderers that are not used.
    /// </summary>
    public void Evict()
    {
        Dictionary<(string, Font), Entry> newStrings = new();

        foreach (KeyValuePair<(string, Font), Entry> pair in strings)
            if (pair.Value.Accessed) // todo: experiment with giving textures points that decay over time (or increase when used) - current system is essentially just one point max
            {
                pair.Value.Accessed = false;
                newStrings.Add(pair.Key, pair.Value);
            }
            else
            {
                pair.Value.Renderer.Dispose();
            }

        strings = newStrings;
    }

    /// <summary>
    ///     Clear the cache, disposing all renderers.
    /// </summary>
    public void Flush()
    {
        foreach (Entry entry in strings.Values) entry.Renderer.Dispose();
        strings.Clear();
    }

    private sealed class Entry
    {
        public Entry(TextRenderer renderer)
        {
            Renderer = renderer;
            Accessed = true;
        }

        public TextRenderer Renderer { get; }
        public bool Accessed { get; set; }
    }
}
