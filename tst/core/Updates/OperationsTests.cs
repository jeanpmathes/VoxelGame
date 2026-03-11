// <copyright file="OperationsTests.cs" company="VoxelGame">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using VoxelGame.Core.App;
using VoxelGame.Core.Updates;
using Xunit;

namespace VoxelGame.Core.Tests.Updates;

[TestSubject(typeof(Operations))]
public sealed class OperationsTests : IDisposable
{
    private readonly UpdateDispatch dispatch = new(singleton: false, Application.Instance);

    public void Dispose()
    {
        dispatch.Dispose();
    }

    [Fact]
    public void Operations_CreateDone_ShouldBeCompleted()
    {
        Operation operation = Operations.CreateDone();

        Assert.True(operation.IsOk);
    }

    [Fact]
    public void Operations_CreateDone_ShouldReturnResult()
    {
        const Int32 result = 42;

        Operation<Int32> operation = Operations.CreateDone(result);

        Assert.True(operation.IsOk);
        Assert.Equal(result, operation.Result?.UnwrapOrThrow());
    }

    [Fact]
    public void Operations_Launch_ShouldBeRunning()
    {
        Operation operation = Operations.Launch(async _ => await Task.CompletedTask.InAnyContext(), dispatch);

        Assert.True(operation.IsRunning);

        dispatch.CompleteAll();
    }

    [Fact]
    public void Operations_Launch_ShouldExecuteAction()
    {
        Boolean executed = false;

        Operation operation = Operations.Launch(async _ =>
            {
                executed = true;
                await Task.CompletedTask.InAnyContext();
            },
            dispatch);

        dispatch.CompleteAll();

        Assert.True(executed);
        Assert.True(operation.IsOk);
    }

    [Fact]
    public void Operations_Launch_ShouldHandleFailure()
    {
        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();

                throw new InvalidOperationException();
            },
            dispatch);

        dispatch.CompleteAll();

        Assert.True(operation.IsFailedOrCancelled);
    }

    [Fact]
    public void Operations_Launch_ShouldHandleCancellation()
    {
        Operation operation = Operations.Launch(async token =>
            {
                await Task.Delay(millisecondsDelay: -1, token).InAnyContext();
            },
            dispatch);

        operation.Cancel();
        dispatch.CompleteAll();

        Assert.True(operation.IsFailedOrCancelled);
    }

    [Fact]
    public void Operations_Then_ShouldPropagateFailure()
    {
        Boolean thenExecuted = false;

        Operation operation = Operations.Launch(async _ =>
                {
                    await Task.CompletedTask.InAnyContext();

                    throw new InvalidOperationException();
                },
                dispatch)
            .Then(async _ =>
            {
                thenExecuted = true;

                await Task.CompletedTask.InAnyContext();
            });

        dispatch.CompleteAll();

        Assert.False(thenExecuted);
        Assert.True(operation.IsFailedOrCancelled);
    }

    [Fact]
    public void Operations_Then_ShouldPropagateCancellation()
    {
        Boolean thenExecuted = false;

        Operation operation = Operations.Launch(async token =>
                {
                    await Task.Delay(millisecondsDelay: -1, token).InAnyContext();
                },
                dispatch)
            .Then(async _ =>
            {
                thenExecuted = true;

                await Task.CompletedTask.InAnyContext();
            });

        operation.Cancel();
        dispatch.CompleteAll();

        Assert.False(thenExecuted);
        Assert.True(operation.IsFailedOrCancelled);
    }

    [Fact]
    public void Operations_Then_ShouldNotRunOnFailure()
    {
        Boolean thenExecuted = false;

        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();

                throw new InvalidOperationException();
            },
            dispatch);

        dispatch.CompleteAll();

        operation = operation.Then(async _ =>
        {
            thenExecuted = true;
            await Task.CompletedTask.InAnyContext();
        });

        dispatch.CompleteAll();

        Assert.False(thenExecuted);
        Assert.True(operation.IsFailedOrCancelled);
    }

    [Fact]
    public void Operations_Then_ShouldNotRunOnCancellation()
    {
        Boolean thenExecuted = false;

        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();
            },
            dispatch);

        operation.Cancel();
        dispatch.CompleteAll();

        operation = operation.Then(async _ =>
        {
            thenExecuted = true;
            await Task.CompletedTask.InAnyContext();
        });

        dispatch.CompleteAll();

        Assert.False(thenExecuted);
        Assert.True(operation.IsFailedOrCancelled);
    }

    [Fact]
    public void Operations_ThenWithResult_ShouldPropagateResult()
    {
        const Int32 initialResult = 42;
        const String nextResult = "success";

        Boolean thenExecuted = false;

        Operation<String> operation = Operations.Launch(async _ =>
                {
                    await Task.CompletedTask.InAnyContext();

                    return initialResult;
                },
                dispatch)
            .Then(async (result, _) =>
            {
                await Task.CompletedTask.InAnyContext();
                thenExecuted = true;
                Assert.Equal(initialResult, result);

                return nextResult;
            });

        dispatch.CompleteAll();

        Assert.True(thenExecuted);
        Assert.True(operation.IsOk);
        Assert.Equal(nextResult, operation.Result?.UnwrapOrThrow());
    }

    [Fact]
    public void Operations_OnCompletionSync_ShouldExecuteOnSuccess()
    {
        Boolean initialExecuted = false;
        Boolean successExecuted = false;
        Boolean failExecuted = false;

        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();
            },
            dispatch);

        operation.OnCompletionSync(
            status =>
            {
                initialExecuted = true;
                Assert.Equal(Status.Ok, status);
            },
            () => successExecuted = true,
            _ => failExecuted = true);

        dispatch.CompleteAll();

        Assert.True(initialExecuted);
        Assert.True(successExecuted);
        Assert.False(failExecuted);
    }

    [Fact]
    public void Operations_OnCompletionSync_ShouldExecuteOnFailure()
    {
        Boolean initialExecuted = false;
        Boolean successExecuted = false;
        Boolean failExecuted = false;

        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();

                throw new InvalidOperationException();
            },
            dispatch);

        operation.OnCompletionSync(
            status =>
            {
                initialExecuted = true;
                Assert.Equal(Status.ErrorOrCancel, status);
            },
            () => successExecuted = true,
            _ => failExecuted = true);

        dispatch.CompleteAll();

        Assert.True(initialExecuted);
        Assert.False(successExecuted);
        Assert.True(failExecuted);
    }

    [Fact]
    public void Operations_OnSuccessfulSync_ShouldExecuteOnSuccess()
    {
        Boolean executed = false;

        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();
            },
            dispatch);

        operation.OnSuccessfulSync(() => executed = true);

        dispatch.CompleteAll();

        Assert.True(executed);
    }

    [Fact]
    public void Operations_OnFailedOrCancelledSync_ShouldExecuteOnFailure()
    {
        Exception exception = new InvalidOperationException();
        Boolean executed = false;

        Operation operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();

                throw exception;
            },
            dispatch);

        operation.OnFailedOrCancelledSync(ex =>
        {
            executed = true;
            Assert.Same(exception, ex);
        });

        dispatch.CompleteAll();

        Assert.True(executed);
    }

    [Fact]
    public void Operations_OnCompletionSyncWithResult_ShouldExecuteOnSuccess()
    {
        const Int32 result = 42;

        Boolean initialExecuted = false;
        Boolean successExecuted = false;
        Boolean failExecuted = false;

        Int32 capturedResult = 0;

        Operation<Int32> operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();

                return result;
            },
            dispatch);

        operation.OnCompletionSync(
            status =>
            {
                initialExecuted = true;
                Assert.Equal(Status.Ok, status);
            },
            value =>
            {
                successExecuted = true;
                capturedResult = value;
            },
            _ => failExecuted = true);

        dispatch.CompleteAll();

        Assert.True(initialExecuted);
        Assert.True(successExecuted);
        Assert.False(failExecuted);
        Assert.Equal(result, capturedResult);
    }

    [Fact]
    public void Operations_OnSuccessfulSyncWithResult_ShouldExecuteOnSuccess()
    {
        const Int32 result = 42;

        Boolean executed = false;
        Int32 capturedResult = 0;

        Operation<Int32> operation = Operations.Launch(async _ =>
            {
                await Task.CompletedTask.InAnyContext();

                return result;
            },
            dispatch);

        operation.OnSuccessfulSync(value =>
        {
            executed = true;
            capturedResult = value;
        });

        dispatch.CompleteAll();

        Assert.True(executed);
        Assert.Equal(result, capturedResult);
    }
}
