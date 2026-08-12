// <copyright file="CompletedOperation.cs" company="VoxelGame">
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
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Updates;

public static partial class Operations
{
    private sealed class CompletedOperation<T> : Operation<T>
    {
        private readonly Result<T> result;

        /// <summary>
        ///     Create a new wrapper operation that directly completes with a result.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="dispatch">The update dispatch.</param>
        public CompletedOperation(T result, UpdateDispatch dispatch) : base(dispatch)
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
                },
                dispatch);
        }

        public override Operation<TNext> Then<TNext>(Func<T, CancellationToken, Task<TNext>> function)
        {
            return Launch(async token => await function(result.UnwrapOrThrow(), token).InAnyContext(), dispatch);
        }
    }
}
