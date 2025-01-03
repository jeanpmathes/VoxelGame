// <copyright file="Operations.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Utility class to work with operations.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Not disposing tasks is fine here.")]
public static class Operations
{
    private static void RegisterOperation(Operation operation)
    {
        Debug.Assert(OperationUpdateDispatch.Instance != null);

        OperationUpdateDispatch.Instance.Add(operation);
    }

    /// <summary>
    ///     Launch an action as an operation.
    ///     It will run on a background thread.
    /// </summary>
    public static Operation Launch(Action action)
    {
        TaskOperation operation = new(action);

        RegisterOperation(operation);

        return operation;
    }

    /// <summary>
    ///     Launch a function as an operation.
    ///     The result will be available when the operation is finished.
    /// </summary>
    public static Operation<T> Launch<T>(Func<T> function)
    {
        TaskOperation<T> operation = new(function);

        RegisterOperation(operation);

        return operation;
    }

    /// <summary>
    ///     Create an operation that is done immediately.
    /// </summary>
    /// <returns>The operation.</returns>
    public static Operation CreateDone()
    {
        return new WrapperOperation<Int32>(result: 0);
    }

    /// <summary>
    ///     Create an operation that is done immediately.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The operation.</returns>
    public static Operation<T> CreateDone<T>(T result)
    {
        return new WrapperOperation<T>(result);
    }

    private sealed class TaskOperation : Operation
    {
        private readonly Action work;

        private readonly Task? previous;
        private Task? current;

        public TaskOperation(Action action, Task? previous = null)
        {
            this.previous = previous;

            work = () =>
            {
                Exception? exception = null;

                try
                {
                    action();
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    Complete(exception);
                }
            };
        }

        protected override void Run()
        {
            current = previous == null ? Task.Run(work) : previous.ContinueWith(_ => work());
        }

        /// <summary>
        ///     Perform an action directly after the operation has successfully completed.
        ///     Might run on a background thread.
        ///     Not all operations support this.
        /// </summary>
        /// <param name="action">The action to perform. Will only run if the operation was successful.</param>
        /// <returns>The operation that runs the action.</returns>
        public override Operation Then(Action action)
        {
            Operation next = new TaskOperation(() =>
                {
                    if (IsWorkStatusOk)
                        action();
                },
                current);

            RegisterOperation(next);

            return next;
        }
    }

    private sealed class TaskOperation<T> : Operation<T>
    {
        private readonly Action work;

        private readonly Task? previous;
        private Task? current;

        public TaskOperation(Func<T> function, Task? previous = null)
        {
            this.previous = previous;

            work = () =>
            {
                try
                {
                    T result = function();
                    Complete(result);
                }
                catch (Exception e)
                {
                    Complete(e);
                }
            };
        }

        protected override void Run()
        {
            current = previous == null ? Task.Run(work) : previous.ContinueWith(_ => work());
        }

        public override Operation Then(Action action)
        {
            Operation next = new TaskOperation(() =>
                {
                    if (IsWorkStatusOk)
                        action();
                },
                current);

            RegisterOperation(next);

            return next;
        }

        public override Operation<TNext> Then<TNext>(Func<T, TNext> function)
        {
            Operation<TNext> next = new TaskOperation<TNext>(() => IsWorkStatusOk ? function(WorkResult!) : default!, current);

            RegisterOperation(next);

            return next;
        }
    }

    private sealed class WrapperOperation<T> : Operation<T>
    {
        /// <summary>
        ///     Create a new wrapper operation that directly completes with a result.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        public WrapperOperation(T result)
        {
            CompleteWith(result);
        }

        /// <summary>
        ///     Complete the operation with a result.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        private void CompleteWith(T result)
        {
            Complete(result);
            WaitForCompletion();
        }

        /// <inheritdoc />
        protected override void Run()
        {
            // Nothing to do here.
        }

        /// <inheritdoc />
        public override Operation Then(Action action)
        {
            action();

            return CreateDone();
        }

        /// <inheritdoc />
        public override Operation<TNext> Then<TNext>(Func<T, TNext> function)
        {
            TNext next = function(WorkResult!);

            return CreateDone(next);
        }
    }
}
