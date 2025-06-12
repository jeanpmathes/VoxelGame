// <copyright file="Operations.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Utility class to work with operations.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Not disposing tasks is fine here.")]
public static class Operations
{
    private const String NoDispatchMessage = "No global dispatch available.";

    private static void RegisterOperation(Operation operation, OperationUpdateDispatch dispatch)
    {
        dispatch.Add(operation);
    }

    /// <summary>
    ///     Launch an action as an operation.
    ///     The action should be async code.
    ///     It will run on a background thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the global dispatch will be used.</param>
    public static Operation Launch(Func<CancellationToken, Task> action, OperationUpdateDispatch? dispatch = null)
    {
        dispatch ??= OperationUpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

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
    /// <param name="dispatch">The dispatch to use for the operation. If <c>null</c>, the global dispatch will be used.</param>
    public static Operation<T> Launch<T>(Func<CancellationToken, Task<T>> function, OperationUpdateDispatch? dispatch = null)
    {
        dispatch ??= OperationUpdateDispatch.Instance ?? throw Exceptions.InvalidOperation(NoDispatchMessage);

        FutureOperation<T> operation = new(function, dispatch);

        RegisterOperation(operation, dispatch);

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

    #pragma warning disable S2931 // Dispose is called in Cleanup, which runs on completion. Implementing IDisposable would harm the Operation interface.
    private sealed class FutureOperationInternal
    #pragma warning restore S2931
    {
        private Future? future;

        private CancellationTokenSource? cancellation;
        private Boolean cancelled;

        public FutureOperationInternal()
        {
            cancellation = new CancellationTokenSource();
            Token = cancellation.Token;
        }

        public CancellationToken Token { get; }

        public Future Run(Func<Task> work, FutureOperationInternal? previous)
        {
            if (previous?.cancelled == true)
                return Future.CreateCanceled();

            CancellationToken token = previous?.Token ?? Token;

            Future running = previous?.future == null
                ? Future.Create(work, token)
                : Future.CreateContinuation(previous.future, work, token);

            future = running;

            return running;
        }

        public Future<T> Run<T>(Func<Task<T>> work, FutureOperationInternal? previous)
        {
            if (previous?.cancelled == true)
                return Future.CreateCanceled<T>();

            CancellationToken token = previous?.Token ?? Token;

            Future<T> running = previous?.future == null
                ? Future.Create(work, token)
                : Future.CreateContinuation(previous.future, work, token);

            future = running;

            return running;
        }

        public void Cleanup()
        {
            ApplicationInformation.ThrowIfNotOnMainThread(this);

            cancellation?.Dispose();
            cancellation = null;
        }

        public void Cancel()
        {
            ApplicationInformation.ThrowIfNotOnMainThread(this);

            cancelled = true;
            cancellation?.Cancel();
        }
    }

    private sealed class FutureOperation : Operation
    {
        private readonly OperationUpdateDispatch dispatch;
        private readonly Func<Task> work;

        private readonly FutureOperationInternal current;
        private readonly FutureOperationInternal? previous;

        private Future? future;

        public FutureOperation(Func<CancellationToken, Task> action, OperationUpdateDispatch dispatch, FutureOperationInternal? previous = null)
        {
            this.dispatch = dispatch;
            this.previous = previous;

            current = new FutureOperationInternal();

            work = async () => await action(current.Token).InAnyContext();
        }

        protected override void Run()
        {
            future = current.Run(work, previous);
        }

        protected override Result? CheckCompletion()
        {
            Debug.Assert(future != null);

            return future.Result;
        }

        protected override Result DoWait()
        {
            Debug.Assert(future != null);

            return future.Wait();
        }

        protected override void OnCompletion()
        {
            current.Cleanup();
        }

        public override void Cancel()
        {
            current.Cancel();
            previous?.Cancel();
        }

        public override Operation Then(Func<CancellationToken, Task> action)
        {
            Operation next = new FutureOperation(token =>
                {
                    Debug.Assert(future != null);
                    Debug.Assert(future.Result != null);

                    return future.Result.Switch(
                        () => action(token),
                        exception => throw exception);
                },
                dispatch,
                current);

            RegisterOperation(next, dispatch);

            return next;
        }
    }

    private sealed class FutureOperation<T> : Operation<T>
    {
        private readonly OperationUpdateDispatch dispatch;
        private readonly Func<Task<T>> work;

        private readonly FutureOperationInternal current;
        private readonly FutureOperationInternal? previous;

        private Future<T>? future;

        public FutureOperation(Func<CancellationToken, Task<T>> function, OperationUpdateDispatch dispatch, FutureOperationInternal? previous = null)
        {
            this.dispatch = dispatch;
            this.previous = previous;

            current = new FutureOperationInternal();

            work = async () => await function(current.Token).InAnyContext();
        }

        protected override void Run()
        {
            future = current.Run(work, previous);
        }

        protected override Result<T>? CheckCompletionT()
        {
            Debug.Assert(future != null);

            return future.Result;
        }

        protected override Result<T> DoWaitT()
        {
            Debug.Assert(future != null);

            return future.Wait();
        }

        protected override void OnCompletion()
        {
            current.Cleanup();
        }

        public override void Cancel()
        {
            current.Cancel();
            previous?.Cancel();
        }

        public override Operation Then(Func<CancellationToken, Task> action)
        {
            Operation next = new FutureOperation(token =>
                {
                    Debug.Assert(future != null);
                    Debug.Assert(future.Result != null);

                    return future.Result.Switch(
                        _ => action(token),
                        exception => throw exception);
                },
                dispatch,
                current);

            RegisterOperation(next, dispatch);

            return next;
        }

        public override Operation<TNext> Then<TNext>(Func<T, CancellationToken, Task<TNext>> function)
        {
            Operation<TNext> next = new FutureOperation<TNext>(token =>
                {
                    Debug.Assert(future != null);
                    Debug.Assert(future.Result != null);

                    return future.Result.Switch(
                        result => function(result, token),
                        exception => throw exception);
                },
                dispatch,
                current);

            RegisterOperation(next, dispatch);

            return next;
        }
    }

    private sealed class WrapperOperation<T> : Operation<T>
    {
        private readonly Result<T> result;

        /// <summary>
        ///     Create a new wrapper operation that directly completes with a result.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        public WrapperOperation(T result)
        {
            this.result = Utilities.Result.Ok(result);

            Wait(); // Force immediate completion.
        }

        /// <inheritdoc />
        protected override void Run()
        {
            // Nothing to do here.
        }

        protected override Result<T> CheckCompletionT()
        {
            return result;
        }

        protected override Result<T> DoWaitT()
        {
            return result;
        }

        public override void Cancel()
        {
            // Nothing to do here.
        }

        public override Operation Then(Func<CancellationToken, Task> action)
        {
            return Launch(async token =>
            {
                await action(token).InAnyContext();
            });
        }

        public override Operation<TNext> Then<TNext>(Func<T, CancellationToken, Task<TNext>> function)
        {
            return Launch(async token => await function(result.UnwrapOrThrow(), token).InAnyContext());
        }
    }
}
