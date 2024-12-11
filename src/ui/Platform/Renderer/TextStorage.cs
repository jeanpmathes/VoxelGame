// <copyright file="TextStorage.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using Gwen.Net;
using VoxelGame.Core.Collections;
using VoxelGame.Toolkit.Utilities;
using Font = Gwen.Net.Font;
using Point = Gwen.Net.Point;
using Size = Gwen.Net.Size;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Stores text renderers and their associated text for reuse.
/// </summary>
public sealed class TextStorage : IDisposable
{
    private readonly DirectXRenderer rendering;
    private readonly DisposableCache<(String, Font), TextRenderer> cache = new(capacity: 200);

    private Dictionary<(String, Font), Entry> used = new();

    /// <summary>
    ///     Creates a new text cache.
    /// </summary>
    public TextStorage(DirectXRenderer rendering)
    {
        StringFormat = new StringFormat(StringFormat.GenericTypographic);
        StringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

        this.rendering = rendering;
    }

    /// <summary>
    ///     Get the string format used by the cache.
    /// </summary>
    public StringFormat StringFormat { get; }

    /// <summary>
    ///     Get the texture for the given text and font if it exists, otherwise return null.
    /// </summary>
    public Texture? GetTexture(Font font, String text)
    {
        Throw.IfDisposed(disposed);

        if (used.TryGetValue((text, font), out Entry? entry))
        {
            entry.Accessed = true;

            return entry.Renderer.Texture;
        }

        if (cache.TryGet((text, font), out TextRenderer? renderer, remove: true))
        {
            Entry newEntry = new(renderer);
            used[(text, font)] = newEntry;

            return newEntry.Renderer.Texture;
        }

        return null;
    }

    /// <summary>
    ///     Get or create the texture for the given text and font.
    /// </summary>
    public Texture GetOrCreateTexture(Font font, String text)
    {
        Throw.IfDisposed(disposed);

        Texture? texture = GetTexture(font, text);

        if (texture != null) return texture;

        Size size = rendering.MeasureText(font, text);
        Entry entry = new(new TextRenderer(size.Width, size.Height, rendering));

        entry.Renderer.SetString(text, (System.Drawing.Font) font.RendererData!, Brushes.White, Point.Zero, StringFormat);
        used[(text, font)] = entry;

        return entry.Renderer.Texture;
    }

    /// <summary>
    ///     Go trough the storage and perform cleanup.
    ///     This can remove renderers that are not used anymore.
    /// </summary>
    public void Update()
    {
        Throw.IfDisposed(disposed);

        Dictionary<(String, Font), Entry> newStrings = new();

        foreach (KeyValuePair<(String, Font), Entry> pair in used)
            if (pair.Value.Accessed)
            {
                pair.Value.Accessed = false;
                newStrings.Add(pair.Key, pair.Value);
            }
            else
            {
                cache.Add(pair.Key, pair.Value.Renderer);
            }

        used = newStrings;
    }

    /// <summary>
    ///     Clear the storage, disposing all renderers.
    /// </summary>
    public void Flush()
    {
        Throw.IfDisposed(disposed);

        foreach (Entry entry in used.Values) entry.Renderer.Dispose();
        used.Clear();

        cache.Flush();
    }

    private sealed class Entry
    {
        public Entry(TextRenderer renderer)
        {
            Renderer = renderer;
            Accessed = true;
        }

        public TextRenderer Renderer { get; }
        public Boolean Accessed { get; set; }
    }

    #region IDisposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Flush();

            StringFormat.Dispose();

            cache.Dispose();
        }

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~TextStorage()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
