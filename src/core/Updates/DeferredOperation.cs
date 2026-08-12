// <copyright file="DeferredOperation.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Core.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Updates;

public static partial class Operations
{
    private sealed class DeferredOperation<T>(Func<T> function, UpdateDispatch dispatch) : Operation<T>(dispatch)
    {
        private Result<T>? result;

        protected override void Run()
        {
            // Because this is not always called in the logic update cycle, there is nothing to do yet.
        }

        protected override Result<T>? CheckCompletionT()
        {
            EnsureCompleted();

            return result;
        }

        protected override Result<T> DoWaitT()
        {
            EnsureCompleted();

            return result;
        }

        [MemberNotNull(nameof(result))]
        private void EnsureCompleted()
        {
            if (result != null) return;

            try
            {
                result = Utilities.Result.Ok(function());
            }
            catch (Exception e)
            {
                result = Utilities.Result.Error<T>(e);
            }
        }

        public override void Cancel()
        {
            if (result != null) return;

            result = Utilities.Result.Error<T>(new OperationCanceledException());
        }

        public override Operation Then(Func<CancellationToken, Task> action)
        {
            if (result != null)
                return result.Switch(() => Launch(async token => await action(token).InAnyContext(), dispatch), _ => this);

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
                held.Release(result!);
            };

            return held;
        }

        public override Operation<TNext> Then<TNext>(Func<T, CancellationToken, Task<TNext>> continuationFunction)
        {
            if (result != null)
                return result.Switch(value => Launch(async token => await continuationFunction(value, token).InAnyContext(), dispatch), exception => new FailedOperation<TNext>(exception, dispatch));

            HeldOperation<T, TNext> held = new(continuationFunction, dispatch, this);

            dispatch.Add(held);

            Completion += (_, _) =>
            {
                held.Release(result!);
            };

            return held;
        }
    }
}
