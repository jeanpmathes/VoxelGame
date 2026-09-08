// <copyright file="Renderer.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals.Colors;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition.UserInterface;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects.UserInterface;

/// <summary>
///     Native user-interface renderer wrapper.
/// </summary>
[NativeMarshalling(typeof(RendererMarshaller))]
public sealed class Renderer : DisposableNativeObject<Renderer>
{
    internal Renderer(IntPtr nativePointer, Client client) : base(nativePointer, client)
    {
        NativeClient = client;
    }

    internal Client NativeClient { get; }

    /// <summary>
    /// Submit a command span to the renderer.
    /// </summary>
    /// <param name="commands">The commands to submit.</param>
    public void Submit(ReadOnlySpan<Command> commands)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        unsafe
        {
            fixed (Command* commandPointer = commands)
            {
                NativeMethods.SubmitUserInterfaceCommands(this, commandPointer, (UInt32) commands.Length);
            }
        }
    }

    /// <summary>
    /// Creates a solid color brush.
    /// </summary>
    /// <param name="color">The color of the brush to create.</param>
    /// <returns>The created brush.</returns>
    public Brush CreateSolidColorBrush(ColorS color)
    {
        return Native.CreateSolidColorBrush(this, color);
    }

    /// <summary>
    /// Creates a text format.
    /// </summary>
    /// <param name="description">The description of the text format to create.</param>
    /// <returns>The created text format.</returns>
    public TextFormat CreateTextFormat(TextFormatDescription description)
    {
        return Native.CreateTextFormat(this, description);
    }

    /// <summary>
    /// Creates a text that can be rendered.
    /// </summary>
    /// <param name="text">The content of the text.</param>
    /// <param name="format">The format of the text.</param>
    /// <returns>The created text.</returns>
    public Text CreateText(String text, TextFormat format)
    {
        return Native.CreateText(this, text, format);
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
                NativeMethods.FreeUserInterface(this);
        }

        base.Dispose(disposing);
    }

    #endregion DISPOSABLE
}

[CustomMarshaller(typeof(Renderer), MarshalMode.ManagedToUnmanagedIn, typeof(RendererMarshaller))]
internal static class RendererMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Renderer managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
