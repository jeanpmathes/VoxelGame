// <copyright file="Future.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading.Tasks;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A future is an operation that will be completed at some point in the future.
///     This specific class is used to wrap a task that will be completed at some point.
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
    ///     Whether the task has completed.
    /// </summary>
    public Boolean IsCompleted => task.IsCompleted;

    /// <summary>
    ///     Get the exception that was thrown by the task, if any.
    /// </summary>
    public Exception? Exception => task.Exception;

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
    ///     Get the value of the future, or a default value if the future has not completed.
    /// </summary>
    public T? Value => IsCompleted ? task.Result : default;
}
