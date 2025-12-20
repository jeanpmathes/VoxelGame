// <copyright file="TextStorage.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
    private readonly DisposableCache<(String, Font), TextRenderer> cache = new(capacity: 200);
    private readonly DirectXRenderer rendering;

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
        ExceptionTools.ThrowIfDisposed(disposed);

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
        ExceptionTools.ThrowIfDisposed(disposed);

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
        ExceptionTools.ThrowIfDisposed(disposed);

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
        ExceptionTools.ThrowIfDisposed(disposed);

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

    #region DISPOSABLE

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

    #endregion DISPOSABLE
}
