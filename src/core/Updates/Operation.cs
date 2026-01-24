// <copyright file="Operation.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VoxelGame.Core.App;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Updates;

/// <summary>
///     An operation is similar to a task, but runs at least partially on the main thread.
///     This means that it is only considered completed after both the actual work has completed and the main thread has
///     detected this.
/// </summary>
public abstract class Operation : IUpdateableProcess
{
    /// <summary>
    ///     Get the current status of the operation.
    /// </summary>
    protected Status Status { get; private set; } = Status.Created;

    /// <summary>
    ///     Whether the operation is ended and no longer running.
    /// </summary>
    private Boolean IsCompleted => Status != Status.Running;

    /// <summary>
    ///     Whether the operation was successful.
    /// </summary>
    public Boolean IsOk => Status == Status.Ok;

    /// <summary>
    ///     Whether the operation failed or was cancelled.
    /// </summary>
    public Boolean IsFailedOrCancelled => Status == Status.ErrorOrCancel;

    /// <summary>
    ///     Get the result of the operation, or <c>null</c> if the operation is still running.
    /// </summary>
    public Result? Result { get; private set; }

    /// <inheritdoc />
    public Boolean IsRunning => Status == Status.Running;

    /// <inheritdoc />
    public void Update()
    {
        if (IsCompleted)
            return;

        Result = CheckCompletion();

        if (Result == null)
            return;

        Complete(Result);
    }

    /// <summary>
    ///     Attempt to cancel the operation.
    ///     Can lead to the operation being cancelled, but does not guarantee it.
    ///     A cancelled operation will be marked as failed.
    ///     Note that this propagates along continuation operations.
    /// </summary>
    public abstract void Cancel();

    /// <summary>
    ///     Invoked when the operation has completed.
    /// </summary>
    protected event EventHandler? Completion;

    internal void Start()
    {
        Application.ThrowIfNotOnMainThread(this);

        Debug.Assert(Status == Status.Created);

        Status = Status.Running;

        Run();
    }

    /// <summary>
    ///     Start working on the operation.
    /// </summary>
    protected abstract void Run();

    private void Complete(Result result)
    {
        Status = result.Switch(() => Status.Ok, _ => Status.ErrorOrCancel);

        OnCompletion();

        Completion?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Check if the operation is completed.
    ///     Is called on the main thread.
    /// </summary>
    /// <returns>The result of the operation, or <c>null</c> if the operation is still running.</returns>
    protected abstract Result? CheckCompletion();

    /// <summary>
    ///     Called before the <see cref="Completion" /> event is invoked.
    /// </summary>
    protected virtual void OnCompletion() {}

    /// <summary>
    ///     Wait for the operation to complete.
    ///     Blocks and runs the main thread until the operation has completed.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    [MustUseReturnValue]
    public Result Wait()
    {
        Result = DoWait();

        Complete(Result);

        return Result;
    }

    /// <inheritdoc cref="Wait" />
    protected abstract Result DoWait();

    /// <summary>
    ///     Perform an action directly after the operation has successfully completed.
    ///     Cancelling the returned operation will also cancel the previous operation.
    ///     Might run on a background thread.
    /// </summary>
    /// <param name="action">The action to perform. Will only run if the operation was successful.</param>
    /// <returns>The operation that runs the action.</returns>
    public abstract Operation Then(Func<CancellationToken, Task> action);

    /// <summary>
    ///     Run an action when the operation is completed.
    ///     Note that the action will run both when the operation is successful and when it fails or is cancelled.
    ///     The action will run on the main thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    public void OnCompletionSync(Action<Status> action, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            action(Status);
        };
    }

    /// <summary>
    ///     Perform a group of actions when the operation is completed.
    ///     Note that an action is considered completed even if it failed or was cancelled.
    ///     The actions will run on the main thread.
    /// </summary>
    /// <param name="initial">The initial action to run. Will run before the success or fail action.</param>
    /// <param name="success">The action to run if the operation was successful.</param>
    /// <param name="fail">The action to run if the operation failed.</param>
    /// <param name="token">A cancellation token. If cancelled, the actions will not run.</param>
    public void OnCompletionSync(Action<Status> initial, Action success, Action<Exception> fail, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            initial(Status);

            Result?.Switch(success, fail);
        };
    }

    /// <summary>
    ///     Perform an action when the operation is completed and successful.
    ///     The action will run on the main thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    /// <returns>This for chaining.</returns>
    public Operation OnSuccessfulSync(Action action, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            Result?.Switch(action, _ => {});
        };

        return this;
    }

    /// <summary>
    ///     Perform an action when the operation is completed by error or cancellation.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    public void OnFailedOrCancelledSync(Action<Exception> action, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested || IsOk)
                return;

            Result?.Switch(() => {}, action);
        };
    }
}

/// <summary>
///     A variant of <see cref="Operation" /> that returns a result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public abstract class Operation<T> : Operation
{
    /// <summary>
    ///     Get the result of the operation, or <c>null</c> if the operation is still running.
    /// </summary>
    public new Result<T>? Result { get; private set; }

    /// <inheritdoc />
    protected sealed override Result? CheckCompletion()
    {
        Result = CheckCompletionT();

        return Result;
    }

    /// <summary>
    ///     Wait for the operation to complete.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public new Result<T> Wait()
    {
        _ = base.Wait();

        return Result!;
    }

    /// <summary>
    ///     Check if the operation is completed.
    /// </summary>
    /// <returns>The result of the operation, or <c>null</c> if the operation is still running.</returns>
    protected abstract Result<T>? CheckCompletionT();

    /// <inheritdoc />
    protected override Result DoWait()
    {
        return Result = DoWaitT();
    }

    /// <inheritdoc cref="DoWait" />
    protected abstract Result<T> DoWaitT();

    /// <summary>
    ///     Perform an action directly after the operation has successfully completed.
    ///     Cancelling the returned operation will also cancel the previous operation.
    ///     Might run on a background thread.
    /// </summary>
    /// <param name="function">The action to perform. Will only run if the operation was successful.</param>
    /// <typeparam name="TNext">The type of the result of the action.</typeparam>
    /// <returns>The operation that runs the action.</returns>
    public abstract Operation<TNext> Then<TNext>(Func<T, CancellationToken, Task<TNext>> function);

    /// <summary>
    ///     Perform a group of actions when the operation is completed.
    ///     All actions will run on the main thread.
    /// </summary>
    /// <param name="initial">The initial action to run. Will run before the success or fail action.</param>
    /// <param name="success">The action to run if the operation was successful.</param>
    /// <param name="fail">The action to run if the operation failed.</param>
    /// <param name="token">A cancellation token. If canceled, the actions will not run.</param>
    public void OnCompletionSync(Action<Status> initial, Action<T> success, Action<Exception> fail, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            initial(Status);

            Result?.Switch(success, fail);
        };
    }

    /// <summary>
    ///     Perform a group of actions when the operation is completed.
    ///     All actions will run on the main thread.
    ///     Will throw an exception if the operation was not successful.
    /// </summary>
    /// <param name="initial">The initial action to run. Will run before the success or fail action.</param>
    /// <param name="success">The action to run if the operation was successful.</param>
    /// <param name="token">A cancellation token. If canceled, the actions will not run.</param>
    public void OnCompletionSync(Action<Status> initial, Action<T> success, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            initial(Status);

            Result?.Switch(success, Fail);
        };

        static void Fail(Exception e)
        {
            throw Exceptions.Annotated("Operation failed", e);
        }
    }

    /// <summary>
    ///     Perform an action when the operation is completed and successful.
    ///     The action will run on the main thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    public void OnSuccessfulSync(Action<T> action, CancellationToken token = default)
    {
        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            Result?.Switch(action, _ => {});
        };
    }
}
