// <copyright file="Coroutine.cs" company="VoxelGame">
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
using System.Collections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A type of <see cref="Operation" /> that runs entirely on the main thread but can yield to wait.
///     By default, yielding <c>null</c> will wait for the next update cycle.
/// </summary>
public class Coroutine : IUpdateableProcess
{
    private readonly IEnumerator coroutine;

    private Coroutine(IEnumerable coroutine)
    {
        // ReSharper disable once GenericEnumeratorNotDisposed
        this.coroutine = coroutine.GetEnumerator();
    }

    /// <inheritdoc />
    public void Update()
    {
        IsRunning = coroutine.MoveNext();

        if (IsRunning)
        {
            // Nothing to do.
        }
        else if (coroutine is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <inheritdoc />
    public void Cancel()
    {
        IsRunning = false;

        if (coroutine is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <inheritdoc />
    public Boolean IsRunning { get; private set; }

    /// <summary>
    ///     Starts a new coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to start.</param>
    public static void Start(Func<IEnumerable> coroutine)
    {
        if (UpdateDispatch.Instance == null)
            throw Exceptions.InvalidOperation($"Cannot start coroutine when no global {nameof(UpdateDispatch)} is available.");

        UpdateDispatch.Instance.Add(new Coroutine(coroutine()));
    }

    /// <summary>
    ///     Starts a new coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to start.</param>
    public static void Start(IEnumerable coroutine)
    {
        if (UpdateDispatch.Instance == null)
            throw Exceptions.InvalidOperation($"Cannot start coroutine when no global {nameof(UpdateDispatch)} is available.");

        UpdateDispatch.Instance.Add(new Coroutine(coroutine));
    }
}
