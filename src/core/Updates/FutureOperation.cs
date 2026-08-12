// <copyright file="FutureOperation.cs" company="VoxelGame">
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
using VoxelGame.Core.App;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Updates;

public static partial class Operations
{
    #pragma warning disable S2931 // Dispose is called in Cleanup, which runs on completion. Implementing IDisposable would harm the Operation interface.
    private sealed class FutureOperationInternal
    #pragma warning restore S2931
    {
        private CancellationTokenSource? cancellation;
        private Boolean cancelled;
        private Future? future;

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
            Application.ThrowIfNotOnMainThread(this);

            cancellation?.Dispose();
            cancellation = null;
        }

        public void Cancel()
        {
            Application.ThrowIfNotOnMainThread(this);

            cancelled = true;
            cancellation?.Cancel();
        }
    }

    private sealed class FutureOperation : Operation
    {
        private readonly FutureOperationInternal current;
        private readonly FutureOperationInternal? previous;

        private readonly Func<Task> work;

        private Future? future;

        public FutureOperation(Func<CancellationToken, Task> action, UpdateDispatch dispatch, FutureOperationInternal? previous = null) : base(dispatch)
        {
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
        private readonly FutureOperationInternal current;
        private readonly FutureOperationInternal? previous;

        private readonly Func<Task<T>> work;

        private Future<T>? future;

        public FutureOperation(Func<CancellationToken, Task<T>> function, UpdateDispatch dispatch, FutureOperationInternal? previous = null) : base(dispatch)
        {
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
}
