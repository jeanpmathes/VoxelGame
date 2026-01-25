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
///     A type of <see cref="IUpdateableProcess" /> that runs entirely on the main thread and can yield to wait.
///     By default, yielding <c>null</c> will wait for the next update cycle.
/// </summary>
public class Coroutine : IUpdateableProcess
{
    private const String NoDispatchMessage = "No global dispatch available.";

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
    /// <param name="dispatch">The dispatch to add the coroutine to. If <c>null</c>, the global dispatch will be used.</param>
    public static void Start(Func<IEnumerable> coroutine, UpdateDispatch? dispatch = null)
    {
        Start(coroutine(), dispatch);
    }

    /// <summary>
    ///     Starts a new coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to start.</param>
    /// <param name="dispatch">The dispatch to add the coroutine to. If <c>null</c>, the global dispatch will be used.</param>
    public static void Start(IEnumerable coroutine, UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);
        dispatch.Add(new Coroutine(coroutine));
    }
}
