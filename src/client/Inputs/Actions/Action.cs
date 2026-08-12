// <copyright file="Action.cs" company="VoxelGame">
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
using VoxelGame.Client.Inputs.Internals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs.Actions;

/// <summary>
///     Base class for state read from a client keybind.
///     Actions in <see cref="Layer.Application" /> update during variable-rate input updates,
///     while actions in <see cref="Layer.Game" /> update before each fixed logic update.
///     In any case, actions remain stable during an update.
///     Dispose an action when its consumer stops using it.
/// </summary>
public abstract class Action : IDisposable
{
    private readonly Binding binding;
    private Boolean disposed;

    /// <summary>
    ///     Create an action that receives input from a binding until it is disposed.
    /// </summary>
    /// <param name="binding">The binding that provides the action's input.</param>
    private protected Action(Binding binding)
    {
        this.binding = binding;
    }

    /// <summary>
    ///     Apply all changes received since the previous update, computing the new state.
    /// </summary>
    /// <param name="change">The state and transitions received since the previous update.</param>
    internal abstract void Apply(ActionStateChange change);

    /// <summary>
    ///     Apply an update without active input or transitions.
    /// </summary>
    internal virtual void ApplyNeutral()
    {
        Apply(ActionStateChange.Neutral);
    }

    #region DISPOSABLE

    /// <summary>
    ///     Called by the finalizer or <see cref="Dispose()" />.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> if called from <see cref="Dispose()" />, <see langword="false" /> if called from the
    ///     finalizer.
    /// </param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) binding.RemoveAction(this);
        else ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Action()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
