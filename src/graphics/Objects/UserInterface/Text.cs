// <copyright file="Text.cs" company="VoxelGame">
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
using System.Drawing;
using System.Runtime.InteropServices.Marshalling;
using VoxelGame.GUI.Texts;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects.UserInterface;

/// <summary>
///     Native user-interface text wrapper.
/// </summary>
[NativeMarshalling(typeof(TextMarshaller))]
public sealed class Text : NativeObject, IFormattedText
{
    private Boolean disposed;
    private SizeF? lastAvailableSize;
    private readonly SizeF lastMeasuredSize = SizeF.Empty;

    internal Text(IntPtr nativePointer, Renderer renderer) : base(nativePointer, renderer.NativeClient)
    {
        Renderer = renderer;
    }

    internal Renderer Renderer { get; }

    /// <inheritdoc />
    public SizeF Measure(SizeF availableSize)
    {
        // todo: Call NativeMethods.MeasureUserInterfaceText and cache the last available size and measured size.
        // Contract: preserve current managed semantics for empty strings and invalid or zero available sizes; scaling is
        // handled in managed code before native measurement or by inversely transforming native results.
        ExceptionTools.ThrowIfDisposed(disposed);
        lastAvailableSize = availableSize;
        return lastMeasuredSize;
    }

    /// <inheritdoc />
    public void Draw(RectangleF rectangle, GUI.Graphics.Brush brush)
    {
        // todo: Emit a text command through the presentation renderer.
        // Contract: if the rectangle size differs from the last measured available size, measure again; text drawing is
        // pixel-snapped before command emission.
        ExceptionTools.ThrowIfDisposed(disposed);
        _ = rectangle;
        _ = brush;
        throw new NotImplementedException();
    }

    internal void InvalidateMeasurement()
    {
        // todo: Invalidate the stored measured size when UI scale changes.
        lastAvailableSize = null;
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            // todo: Deregister and call NativeMethods.FreeUserInterfaceText.
            // Contract: native text objects are not reusable and are destroyed when returned to TextSupport.
            throw new NotImplementedException();
        }

        ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Text()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}

[CustomMarshaller(typeof(Text), MarshalMode.ManagedToUnmanagedIn, typeof(TextMarshaller))]
internal static class TextMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Text managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
