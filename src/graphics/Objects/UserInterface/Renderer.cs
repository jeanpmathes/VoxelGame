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
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition.UserInterface;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects.UserInterface;

/// <summary>
///     Native user-interface renderer wrapper.
/// </summary>
[NativeMarshalling(typeof(RendererMarshaller))]
public sealed class Renderer : NativeObject, IDisposable
{
    private Boolean disposed;

    internal Renderer(IntPtr nativePointer, Client client) : base(nativePointer, client)
    {
        NativeClient = client;
    }

    internal Client NativeClient { get; }

    internal void Submit(ReadOnlySpan<Command> commands)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        // todo: Submit the command span to NativeMethods.SubmitUserInterfaceCommands.
        // Contract: throw on use after dispose; the native renderer copies the passed span and the caller retains no
        // ownership after the call.

        throw new NotImplementedException();
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
            // todo: Deregister and call NativeMethods.FreeUserInterface.
            // Contract: after dispose, this wrapper and the native pointer must never be used again.
            throw new NotImplementedException();
        }

        ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Renderer()
    {
        Dispose(disposing: false);
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
