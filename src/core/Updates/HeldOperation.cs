// <copyright file="HeldOperation.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Updates;

public static partial class Operations
{
    private sealed class HeldOperation<TPrevious, T>(Func<TPrevious, CancellationToken, Task<T>> function, UpdateDispatch dispatch, Operation<TPrevious> holding) : Operation<T>(dispatch)
    {
        private Operation<T>? inner;

        protected override void Run()
        {
            // Wait until released.
        }

        public void Release(Result<TPrevious> result)
        {
            if (inner != null) return;

            inner = result.Switch(value => Launch<T>(async token => await function(value, token), dispatch), exception => new FailedOperation<T>(exception, dispatch));
        }

        protected override Result<T>? CheckCompletionT()
        {
            return inner?.Result;
        }

        protected override Result<T> DoWaitT()
        {
            holding.Wait();

            Debug.Assert(inner != null);

            return inner.Wait();
        }

        public override void Cancel()
        {
            holding.Cancel();

            if (inner == null)
                inner = new FailedOperation<T>(new OperationCanceledException(), dispatch);
            else
                inner.Cancel();
        }

        public override Operation Then(Func<CancellationToken, Task> action)
        {
            if (inner != null) return inner.Then(action);

            HeldOperation<T, Void> held = new(async (_, token) =>
                {
                    await action(token);
                    return Void.Instance;
                },
                dispatch,
                this);

            dispatch.Add(held);

            Completion += (_, _) =>
            {
                held.Release(Result!);
            };

            return held;
        }

        public override Operation<TNext> Then<TNext>(Func<T, CancellationToken, Task<TNext>> continuationFunction)
        {
            if (inner != null) return inner.Then(continuationFunction);

            HeldOperation<T, TNext> held = new(continuationFunction, dispatch, this);

            dispatch.Add(held);

            Completion += (_, _) =>
            {
                held.Release(Result!);
            };

            return held;
        }
    }
}
