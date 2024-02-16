// <copyright file="Operations.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading.Tasks;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Utility class to work with operations.
/// </summary>
public static class Operations
{
                                                                                                                                                                                                                                                                                                                                                                    #pragma warning disable CA1001 // Not disposing the task is fine in this case.
#pragma warning disable S2931 // Not disposing the task is fine in this case.

    private sealed class TaskOperation : Operation
    {
        private readonly Task task;

        public TaskOperation(Action action)
        {
            task = new Task(() =>
            {
                Exception? exception = null;

                try
                {
                    action();
                }
#pragma warning disable S2221 // Action might throw any exception.
                catch (Exception e)
#pragma warning restore S2221 // Action might throw any exception.
                {
                    exception = e;
                }
                finally
                {
                    Complete(exception);
                }
            });
        }

        protected override void Run()
        {
            task.Start();
        }
    }

    private sealed class TaskOperation<T> : Operation<T>
    {
        private readonly Task task;

        public TaskOperation(Func<T> function)
        {
            task = new Task(() =>
            {
                try
                {
                    T result = function();
                    Complete(result);
                }
#pragma warning disable S2221 // Action might throw any exception.
                catch (Exception e)
#pragma warning restore S2221 // Action might throw any exception.
                {
                    Complete(e);
                }
            });
        }

        protected override void Run()
        {
            task.Start();
        }
    }

    /// <summary>
    ///     Launch an action as an operation.
    ///     It will run on a background thread.
    /// </summary>
    public static Operation Launch(Action action)
    {
        TaskOperation operation = new(action);

        if (OperationUpdateDispatch.Instance == null)
            throw new InvalidOperationException();

        OperationUpdateDispatch.Instance.Add(operation);

        return operation;
    }

    /// <summary>
    ///     Launch a function as an operation.
    ///     The result will be available when the operation is finished.
    /// </summary>
    public static Operation<T> Launch<T>(Func<T> function)
    {
        TaskOperation<T> operation = new(function);

        if (OperationUpdateDispatch.Instance == null)
            throw new InvalidOperationException();

        OperationUpdateDispatch.Instance.Add(operation);

        return operation;
    }
}
