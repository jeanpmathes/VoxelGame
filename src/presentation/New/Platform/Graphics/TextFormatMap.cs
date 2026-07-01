// <copyright file="TextFormatMap.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Core.Collections;
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Texts;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Maps text-format descriptions to text formats, while also managing lifetimes.
/// </summary>
internal sealed class TextFormatMap(Renderer renderer) : IDisposable
{
    private readonly Dictionary<TextOptions, Entry> activeFormats = [];
    private readonly Dictionary<TextFormat, Entry> formatMap = [];

    private readonly Cache<TextOptions, TextFormat> cache = new DisposableCache<TextOptions, TextFormat>(100);

    public TextFormat Request(TextOptions options)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (activeFormats.TryGetValue(options, out Entry? entry))
        {
            entry.usage += 1;

            return entry.format;
        }

        if (!cache.TryGet(options, out TextFormat? format, remove: true))
            format = renderer.CreateTextFormat(options);

        entry = new Entry {format = format, options = options, usage = 1};
        activeFormats.Add(options, entry);
        formatMap.Add(format, entry);

        return format;
    }

    public void Return(TextFormat format)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        Entry entry = formatMap[format];

        entry.usage -= 1;

        if (entry.usage != 0) return;

        activeFormats.Remove(entry.options);
        formatMap.Remove(format);

        cache.Add(entry.options, format);
    }

    private class Entry
    {
        public Int32 usage;
        public TextOptions options;
        public required TextFormat format;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~TextFormatMap()
    {
        Dispose(disposing: false);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            cache.Flush();

            foreach (Entry entry in activeFormats.Values)
            {
                entry.format.Dispose();
            }

            activeFormats.Clear();
            formatMap.Clear();
        }
        else ExceptionTools.ThrowForMissedDispose<TextFormatMap>();

        disposed = true;
    }

    #endregion DISPOSABLE
}
