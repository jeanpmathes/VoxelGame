// <copyright file="Future.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A future is an operation that will be completed at some point in the future.
///     This specific class is used to wrap a task that will be completed at some point.
///     Prefer using <see cref="Operation"/> in most cases; future is to be used as a primitive when building complex systems.
/// </summary>
public class Future
{
    private readonly Task task;

    /// <summary>
    ///     Create a future from a task.
    /// </summary>
    /// <param name="task">The task to wrap.</param>
    protected Future(Task task)
    {
        this.task = task;
    }

    /// <summary>
    ///     Whether the future has completed.
    /// </summary>
    public Boolean IsCompleted => task.IsCompleted;

    /// <summary>
    ///     Whether the future has completed successfully.
    /// </summary>
    public Boolean IsCompletedSuccessfully => task.IsCompletedSuccessfully;

    /// <summary>
    ///     Whether the future has either failed or been cancelled.
    /// </summary>
    public Boolean IsFailedOrCancelled => task.IsFaulted || task.IsCanceled;

    /// <summary>
    /// Get the result if completed, otherwise <c>null</c>.
    /// </summary>
    public Result? Result => IsCompleted ? Wait() : null;

    /// <summary>
    ///     Get the exception that was thrown by the inner work, if any.
    ///     This also includes exceptions from cancellation.
    /// </summary>
    protected Exception? Exception
    {
        get
        {
            if (!task.IsCompleted || task.IsCompletedSuccessfully)
                return null;

            if (task.IsFaulted)
                return task.Exception.GetBaseException();

            if (!task.IsCanceled)
                return null;

            try { task.GetAwaiter().GetResult(); }
            catch (TaskCanceledException e) { return e; }
            catch (OperationCanceledException e) { return e; }

            return null;
        }
    }

    /// <summary>
    ///     Create a future that will be started when the given future is completed.
    /// </summary>
    /// <param name="future">The future to continue from.</param>
    /// <param name="continuation">
    ///     The action to run when the future is completed.
    ///     Will be tracked by the new future.
    /// </param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>The new future.</returns>
    public static Future CreateContinuation(Future future, Func<Task> continuation, CancellationToken token = default)
    {
        return new Future(future.task.ContinueWith(_ => continuation(), token).Unwrap());
    }

    /// <summary>
    ///     Create a future that will be started when the given future is completed.
    /// </summary>
    /// <param name="future">The future to continue from.</param>
    /// <param name="continuation">
    ///     The action to run when the future is completed.
    ///     Will be tracked by the new future.
    /// </param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <typeparam name="T">The type of the future value.</typeparam>
    /// <returns>The new future.</returns>
    public static Future<T> CreateContinuation<T>(Future future, Func<Task<T>> continuation, CancellationToken token = default)
    {
        return new Future<T>(future.task.ContinueWith(_ => continuation(), token).Unwrap());
    }

    /// <summary>
    ///     Create a future from an action.
    ///     The action will be run on a background thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <returns>The future.</returns>
    public static Future Create(Action action)
    {
        return new Future(Task.Run(action));
    }

    /// <summary>
    ///     Run an async action in a future.
    ///     The action will be run on a background thread.
    /// </summary>
    /// <param name="action">The async action to run.</param>
    /// <param name="token">The token to cancel the operation.</param>
    /// <returns>The future.</returns>
    public static Future Create(Func<Task> action, CancellationToken token)
    {
        return new Future(Task.Run(action, token));
    }

    /// <summary>
    ///     Create a future from a function.
    ///     The function will be run on a background thread.
    /// </summary>
    /// <param name="function">The function to run.</param>
    /// <returns>The future.</returns>
    /// <typeparam name="T">The type of the future value.</typeparam>
    public static Future<T> Create<T>(Func<T> function)
    {
        return new Future<T>(Task.Run(function));
    }

    /// <summary>
    ///     Create a future from an async function.
    ///     The function will be run on a background thread.
    /// </summary>
    /// <param name="function">The async function to run.</param>
    /// <param name="token">The token to cancel the operation.</param>
    /// <typeparam name="T">The type of the future value.</typeparam>
    /// <returns>The future.</returns>
    public static Future<T> Create<T>(Func<Task<T>> function, CancellationToken token)
    {
        return new Future<T>(Task.Run(function, token));
    }

    /// <summary>
    ///     Create a canceled future.
    /// </summary>
    /// <returns>The future.</returns>
    public static Future CreateCanceled()
    {
        var source = new TaskCompletionSource<Object>();

        source.SetCanceled();

        return new Future(source.Task);
    }

    /// <summary>
    ///     Create a canceled future.
    /// </summary>
    /// <typeparam name="T">The type of the future value.</typeparam>
    /// <returns>The future.</returns>
    public static Future<T> CreateCanceled<T>()
    {
        var source = new TaskCompletionSource<T>();

        source.SetCanceled();

        return new Future<T>(source.Task);
    }

    /// <summary>
    ///     Wait for the future to complete.
    ///     Will not throw exceptions.
    /// </summary>
    public Result Wait()
    {
        try
        {
            task.GetAwaiter().GetResult();

            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Error(Exception!);
        }
    }
}

/// <summary>
///     A future is a value that will be available at some point in the future.
///     This specific class is used to wrap a task that will be completed at some point.
/// </summary>
/// <typeparam name="T">The type of the future value.</typeparam>
public class Future<T> : Future
{
    private readonly Task<T> task;

    /// <summary>
    ///     Create a future from a task.
    /// </summary>
    /// <param name="task">The task to wrap.</param>
    internal Future(Task<T> task) : base(task)
    {
        this.task = task;
    }

    /// <summary>
    /// Get the result if completed, otherwise null.
    /// </summary>
    public new Result<T>? Result => IsCompleted ? Wait() : null;

    /// <summary>
    ///     Wait for the future to complete, will not throw exceptions.
    /// </summary>
    /// <returns>The value of the future.</returns>
    public new Result<T> Wait()
    {
        try
        {
            T result = task.GetAwaiter().GetResult();

            return Utilities.Result.Ok(result);
        }
        catch (Exception)
        {
            return Utilities.Result.Error<T>(Exception!);
        }
    }
}

/// <summary>
///     Utility methods for futures and tasks.
/// </summary>
public static class FutureExtensions
{
    /// <summary>
    ///     Configure a task to not continue on the captured context.
    /// </summary>
    public static ConfiguredTaskAwaitable InAnyContext(this Task task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    ///     Configure a task to not continue on the captured context.
    /// </summary>
    public static ConfiguredTaskAwaitable<T> InAnyContext<T>(this Task<T> task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    ///     Configure a task to not continue on the captured context.
    /// </summary>
    public static ConfiguredValueTaskAwaitable InAnyContext(this ValueTask task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    ///     Configure a task to not continue on the captured context.
    /// </summary>
    public static ConfiguredValueTaskAwaitable<T> InAnyContext<T>(this ValueTask<T> task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    ///     Configure a task to continue on the captured context.
    /// </summary>
    public static ConfiguredTaskAwaitable InThisContext(this Task task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: true);
    }

    /// <summary>
    ///     Configure a task to continue on the captured context.
    /// </summary>
    public static ConfiguredTaskAwaitable<T> InThisContext<T>(this Task<T> task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: true);
    }

    /// <summary>
    /// Configure a task to continue on the captured context.
    /// </summary>
    public static ConfiguredValueTaskAwaitable InThisContext(this ValueTask task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: true);
    }

    /// <summary>
    ///     Configure a task to continue on the captured context.
    /// </summary>
    public static ConfiguredValueTaskAwaitable<T> InThisContext<T>(this ValueTask<T> task)
    {
        return task.ConfigureAwait(continueOnCapturedContext: true);
    }
}
