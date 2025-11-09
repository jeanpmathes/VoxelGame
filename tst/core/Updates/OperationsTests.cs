// <copyright file="OperationsTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
public class OperationsTests
{
    private readonly OperationUpdateDispatch dispatch = new(singleton: false, Application.Instance);

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
        var executed = false;

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
        var thenExecuted = false;

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
        var thenExecuted = false;

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
        var thenExecuted = false;

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
        var thenExecuted = false;

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

        var thenExecuted = false;

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
        var initialExecuted = false;
        var successExecuted = false;
        var failExecuted = false;

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
        var initialExecuted = false;
        var successExecuted = false;
        var failExecuted = false;

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
        var executed = false;

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
        var executed = false;

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

        var initialExecuted = false;
        var successExecuted = false;
        var failExecuted = false;

        var capturedResult = 0;

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

        var executed = false;
        var capturedResult = 0;

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
