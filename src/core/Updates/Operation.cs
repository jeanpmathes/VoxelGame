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
    private const int Working = (int) Status.Running;

    private int workStatus = Working;

    private Exception? workException;

    private bool started;

    /// <summary>
    ///     Get the current status of the operation.
    /// </summary>
    public Status Status { get; private set; } = Status.Running;

    /// <summary>
    ///     Whether the operation is completed.
    /// </summary>
    private bool IsCompleted => Status != Status.Running;

    /// <summary>
    ///     Whether the operation is currently running.
    /// </summary>
    public bool IsRunning => Status == Status.Running;

    /// <summary>
    ///     Gets the error that occurred during the operation, if any.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    ///     Whether the operation was successful.
    /// </summary>
    public bool IsOk => Status == Status.Ok;

    /// <summary>
    ///     Invoked when the operation has completed.
    /// </summary>
    public event EventHandler Completion = delegate {};

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

        Complete((int) status, exception);
    }

    private void Complete(int status, Exception? exception)
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
    ///     Chain an action to this operation.
    ///     The action will run on the main thread when the operation is completed.
    /// </summary>
    /// <param name="action">The action to chain.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    /// <returns>This operation.</returns>
    public Operation OnCompletion(Action<Operation> action, CancellationToken token = default)
    {
        if (OperationUpdateDispatch.Instance == null)
            throw new InvalidOperationException();

        Completion += (_, _) =>
        {
            if (token.IsCancellationRequested)
                return;

            action(this);
        };

        return this;
    }
}

/// <summary>
///     A variant of <see cref="Operation" /> that returns a result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public abstract class Operation<T> : Operation
{
    private object? workResult;

    /// <inheritdoc />
    protected Operation()
    {
        Completion += (_, _) =>
        {
            var result = (T?) Interlocked.CompareExchange(ref workResult, value: null, comparand: null);

            if (!Equals(result, default(T)))
                Result = result;
        };
    }

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
    ///     Chain an action to this operation.
    /// </summary>
    /// <param name="action">The action to chain.</param>
    /// <param name="token">A cancellation token. If cancelled, the action will not run.</param>
    /// <returns>This operation.</returns>
    public Operation<T> OnCompletion(Action<Operation<T>> action, CancellationToken token = default)
    {
        base.OnCompletion(_ => action(this), token);

        return this;
    }
}
