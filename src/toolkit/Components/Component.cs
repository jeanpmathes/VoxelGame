// <copyright file="Component.cs" company="VoxelGame">
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

namespace VoxelGame.Toolkit.Components;

/// <summary>
///     Base class for components.
///     Components allow composition of functionality in a subject.
/// </summary>
/// <param name="subject">The subject that this component belongs to.</param>
/// <typeparam name="TSubject">The type of the subject.</typeparam>
public abstract class Component<TSubject>(TSubject subject) : IDisposable where TSubject : Composed
{
    /// <summary>
    ///     Get the subject of this component.
    /// </summary>
    public TSubject Subject { get; } = subject;

    /// <summary>
    ///     Remove this component from its subject.
    /// </summary>
    public void RemoveSelf()
    {
        Subject.RemoveComponent(this);
    }

    #region DISPOSABLE

    /// <summary>
    ///     Override this method to dispose of resources.
    /// </summary>
    /// <param name="disposing">Whether the method is being called from Dispose or the finalizer.</param>
    protected virtual void Dispose(Boolean disposing) {}

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Component()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
