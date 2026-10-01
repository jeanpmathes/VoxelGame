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
using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Graphics.Definition.UserInterface;
using VoxelGame.GUI.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects.UserInterface;

/// <summary>
///     Native user-interface text wrapper.
/// </summary>
[NativeMarshalling(typeof(TextMarshaller))]
public sealed class Text : DisposableNativeObject<Text>
{
    private Size? lastAvailableSize;
    private Size lastMeasuredSize = Size.Empty;

    internal Text(IntPtr nativePointer, Renderer renderer) : base(nativePointer, renderer.NativeClient)
    {
        Renderer = renderer;
    }

    internal Renderer Renderer { get; }

    /// <summary>
    /// Sets the text content.
    /// </summary>
    /// <param name="content">The new text content.</param>
    public void SetContent(String content)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        lastAvailableSize = null;
        lastMeasuredSize = Size.Empty;

        Native.SetTextContent(this, content);
    }

    /// <summary>
    /// Sets the text format.
    /// </summary>
    /// <param name="format">The new text format description.</param>
    public void SetFormat(TextFormatDescription format)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        lastAvailableSize = null;
        lastMeasuredSize = Size.Empty;

        Native.SetTextFormat(this, format);
    }

    /// <summary>
    /// Measure the text.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The measured size.</returns>
    public Size Measure(Size availableSize)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (availableSize == lastAvailableSize)
            return lastMeasuredSize;

        lastAvailableSize = availableSize;

        lastMeasuredSize = NativeMethods.MeasureUserInterfaceText(this, availableSize);

        return lastMeasuredSize;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc/>
    protected override void Dispose(Boolean disposing)
    {
        if (!disposed)
        {
            disposed = true;

            if (disposing)
                NativeMethods.ReturnUserInterfaceText(this);
        }

        base.Dispose(disposing);
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
