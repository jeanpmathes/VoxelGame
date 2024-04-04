// <copyright file="Operation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Threading;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Updates;

/// <summary>
///     An operation is similar to a task, but runs at least partially on the main thread.
///     This means that it is only considered completed after both the actual work has completed and the main thread has
///     detected this.
/// </summary>
public abstract class Operation
{
    private const Int32 Working = (Int32) Status.Running;

    private Int32 workStatus = Working;
    private Exception? workException;

    private Boolean started;

    /// <summary>
    ///     Get the current status of the operation.
    /// </summary>
    public Status Status { get; private set; } = Status.Running;

    /// <summary>
    ///     Whether the work-status is OK.
    ///     The work-status is an internal status that becomes the external status after an update.
    ///     If this returns true, the operation is considered successful and will complete on the next update.
    /// </summary>
    protected Boolean IsWorkStatusOk => (Status) Interlocked.CompareExchange(ref workStatus, value: 0, comparand: 0) == Status.Ok;

    /// <summary>
    ///     Whether the operation is completed.
    /// </summary>
    private Boolean IsCompleted => Status != Status.Running;

    /// <summary>
    ///     Whether the operation is currently running.
    /// </summary>
    public Boolean IsRunning => Status == Status.Running;

    /// <summary>
    ///     Gets the error that occurred during the operation, if any.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    ///     Whether the operation was successful.
    /// </summary>
    public Boolean IsOk => Status == Status.Ok;

    /// <summary>
    ///     Invoked when the operation has completed.
    /// </summary>
    protected event EventHandler Completion = delegate {};

    internal void Start()
    {
        Debug.Assert(Status == Status.Running);
        Debug.Assert(!started);

        Throw.IfNotOnMainThread(this);

        started = true;

        Run();
    }

    /// <summary>
    ///     Start working on the operation.
    /// </summary>
    protected abstract void Run();

    /// <summary>
    ///     Complete the operation, either successfully or with an error.
    /// </summary>
    protected void Complete(Exception? exception = null)
    {
        Status status = exception == null ? Status.Ok : Status.Error;

        Complete((Int32) status, exception);
    }

    private void Complete(Int32 status, Exception? exception)
    {
        Interlocked.Exchange(ref workStatus, status);
        Interlocked.Exchange(ref workException, exception);
    }

    /// <summary>
    ///     Is called by <see cref="OperationUpdateDispatch" /> to update the operation.
    /// </summary>
    internal void Update()
    {
        if (IsCompleted)
            return;

        var next = (Status) Interlocked.CompareExchange(ref workStatus, value: 0, comparand: 0);

        if (next == Status.Running)
            return;

        Status = next;
        Error = Interlocked.CompareExchange(ref workException, value: null, comparand: null);

        Completion(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Perform an action directly after the operation has successfully completed.
    ///     Might run on a background thread.
    ///     Not all operations support this.
    /// </summary>
    /// <param name="action">The action to perform. Will only run if the operation was successful.</param>
    /// <returns>The operation that runs the action.</returns>
    public virtual Operation Then(Action action)
    {
        throw new InvalidOperationException();
    }

    /// <summary>
    ///     Wait for the operation to complete.
    ///     Blocks and runs the main thread until the operation has completed.
    /// </summary>
    /// <returns>The exception that occurred during the operation, if any.</returns>
    public Exception? WaitForCompletion()
    {
        while (!IsCompleted)
            Update();

        return Error;
    }

    /// <summary>
    ///     Perform an action when the operation is completed.
    ///     The action will run on the main thread.
    /// </summary>
    /// <param name="action">The action to chain.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    public void OnCompletion(Action<Operation> action, CancellationToken token = default)
    {
        if (OperationUpdateDispatch.Instance == null)
            throw new InvalidOperationException();

        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            action(this);
        };
    }
}

/// <summary>
///     A variant of <see cref="Operation" /> that returns a result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public abstract class Operation<T> : Operation
{
    private Object? workResult;

    /// <inheritdoc />
    protected Operation()
    {
        Completion += (_, _) =>
        {
            T? result = WorkResult;

            if (!Equals(result, default(T)))
                Result = result;
        };
    }

    /// <summary>
    ///     Get the work-result, which is the result of the operation before it is set as the final result.
    ///     Only use this from the thread that runs the operation, or when sure that the operation has completed.
    /// </summary>
    protected T? WorkResult => (T?) Interlocked.CompareExchange(ref workResult, value: null, comparand: null);

    /// <summary>
    ///     The result of the operation.
    /// </summary>
    public T? Result { get; private set; }

    /// <summary>
    ///     Complete the operation with a result.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    protected void Complete(T result)
    {
        Interlocked.Exchange(ref workResult, result);

        Complete();
    }

    /// <summary>
    ///     Perform an action directly after the operation has successfully completed.
    ///     Might run on a background thread.
    ///     Not all operations support this.
    /// </summary>
    /// <param name="function">The action to perform. Will only run if the operation was successful.</param>
    /// <typeparam name="TNext">The type of the result of the action.</typeparam>
    /// <returns>The operation that runs the action.</returns>
    public virtual Operation<TNext> Then<TNext>(Func<T, TNext> function)
    {
        throw new InvalidOperationException();
    }

    /// <summary>
    ///     Wait for the operation to complete.
    ///     Blocks and runs the main thread until the operation has completed.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public new T? WaitForCompletion()
    {
        base.WaitForCompletion();

        return Result;
    }

    /// <summary>
    ///     Perform an action on the main thread when the operation is completed.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    public void OnCompletion(Action<Operation<T>> action, CancellationToken token = default)
    {
        base.OnCompletion(_ => action(this), token);
    }
}
