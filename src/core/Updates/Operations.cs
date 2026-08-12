// <copyright file="Operations.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Utility class to work with operations.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Not disposing tasks is fine here.")]
public static partial class Operations
{
    private const String NoDispatchMessage = "No singleton dispatch available.";

    private static void RegisterOperation(Operation operation, UpdateDispatch dispatch)
    {
        dispatch.Add(operation);
    }

    /// <summary>
    ///     Launch an action as an operation.
    ///     The action should be async code.
    ///     It will run on a background thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the singleton dispatch will be used.</param>
    public static Operation Launch(Func<CancellationToken, Task> action, UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        FutureOperation operation = new(action, dispatch);

        RegisterOperation(operation, dispatch);

        return operation;
    }

    /// <summary>
    ///     Launch a function as an operation.
    ///     The function should be async code.
    ///     The result will be available when the operation is finished.
    /// </summary>
    /// <param name="function">The function to run.</param>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the singleton dispatch will be used.</param>
    public static Operation<T> Launch<T>(Func<CancellationToken, Task<T>> function, UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        FutureOperation<T> operation = new(function, dispatch);

        RegisterOperation(operation, dispatch);

        return operation;
    }

    /// <summary>
    ///     Create an operation that is done immediately.
    /// </summary>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the singleton dispatch will be used.</param>
    /// <returns>The operation.</returns>
    public static Operation CreateDone(UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        return new CompletedOperation<Void>(Void.Instance, dispatch);
    }

    /// <summary>
    ///     Create an operation that is done immediately.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the singleton dispatch will be used.</param>
    /// <returns>The operation.</returns>
    public static Operation<T> CreateDone<T>(T result, UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        return new CompletedOperation<T>(result, dispatch);
    }

    /// <summary>
    ///     Defer an action to run during a logic update.
    ///     This will run as soon as possible, but never immediately in this call.
    ///     This will never run on another thread except the main thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the singleton dispatch will be used.</param>
    /// <returns>The operation.</returns>
    public static Operation Defer(Action action, UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        Operation operation = new DeferredOperation<Void>(action.ToFunction(), dispatch);

        RegisterOperation(operation, dispatch);

        return operation;
    }

    /// <summary>
    ///     Defer a function to run during a logic update.
    ///     This will run as soon as possible, but never immediately in this call.
    ///     This will never run on another thread except the main thread.
    /// </summary>
    /// <param name="function">The function to run.</param>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the singleton dispatch will be used.</param>
    /// <returns>The operation.</returns>
    public static Operation<T> Defer<T>(Func<T> function, UpdateDispatch? dispatch = null)
    {
        dispatch ??= UpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        Operation<T> operation = new DeferredOperation<T>(function, dispatch);

        RegisterOperation(operation, dispatch);

        return operation;
    }
}
