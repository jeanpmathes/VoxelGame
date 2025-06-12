// <copyright file="FutureTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VoxelGame.Core.Updates;
using Xunit;

namespace VoxelGame.Core.Tests.Updates;

[TestSubject(typeof(Future))]
public class FutureTests
{
    [Fact]
    public void Future_Create_ShouldRunAction()
    {
        var ran = false;

        Future.Create(() => { ran = true; }).Wait();

        Assert.True(ran);
    }

    [Fact]
    public void Future_Create_ShouldRunAsyncAction()
    {
        var ran = false;

        Future.Create(async () =>
            {
                ran = true;

                await Task.CompletedTask.InAnyContext();
            },
            CancellationToken.None).Wait();

        Assert.True(ran);
    }

    [Fact]
    public void Future_Create_ShouldRunFunction()
    {
        Int32 result = Future.Create(() => 42).Wait().UnwrapOrThrow();

        Assert.Equal(expected: 42, result);
    }

    [Fact]
    public void Future_Create_ShouldRunAsyncFunction()
    {
        Int32 result = Future.Create(async () =>
            {
                await Task.CompletedTask.InAnyContext();

                return 42;
            },
            CancellationToken.None).Wait().UnwrapOrThrow();

        Assert.Equal(expected: 42, result);
    }

    [Fact]
    public void Future_CreateContinuation_ShouldRunAsyncActionContinuation()
    {
        var initial = Future.Create(() => {});

        initial.Wait();

        var ran = false;

        Future.CreateContinuation(initial,
            async () =>
            {
                ran = true;

                await Task.CompletedTask.InAnyContext();
            }).Wait();

        Assert.True(ran);
    }

    [Fact]
    public void Future_IsCompleted_ShouldBeTrueAfterCompletion()
    {
        var future = Future.Create(() => {});

        future.Wait();

        Assert.True(future.IsCompleted);
    }

    [Fact]
    public void Future_IsCompleted_ShouldBeFalseBeforeCompletion()
    {
        using CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        var future = Future.Create(async () =>
            {
                await Task.Delay(millisecondsDelay: -1, token).InAnyContext();
            },
            token);

        Assert.False(future.IsCompleted);

        source.Cancel();
    }

    [Fact]
    public void Future_IsFailedOrCancelled_ShouldBeFalseAfterCompletion()
    {
        var future = Future.Create(() => {});

        future.Wait();

        Assert.False(future.IsFailedOrCancelled);
    }

    [Fact]
    public void Future_IsFailedOrCancelled_ShouldBeTrueAfterCancellation()
    {
        using CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        var future = Future.Create(async () =>
            {
                await Task.Delay(millisecondsDelay: -1, token).InAnyContext();
            },
            token);

        source.Cancel();
        future.Wait();

        Assert.True(future.IsFailedOrCancelled);
    }

    [Fact]
    public void Future_IsFailedOrCancelled_ShouldBeTrueAfterFailure()
    {
        var future = Future.Create(async () =>
            {
                await Task.CompletedTask.InAnyContext();

                throw new InvalidOperationException();
            },
            CancellationToken.None);

        future.Wait();

        Assert.True(future.IsFailedOrCancelled);
    }

    [Fact]
    public void Future_ShouldHoldExceptionAfterCancellation()
    {
        using CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        var future = Future.Create(async () =>
            {
                await Task.Delay(millisecondsDelay: -1, token).InAnyContext();
            },
            token);

        source.Cancel();

        future.Wait().Switch(
            () => Assert.Fail(),
            _ => {} // Expected.
        );
    }

    [Fact]
    public void Future_ShouldHoldActualExceptionOnFailure()
    {
        InvalidOperationException exception = new();

        var future = Future.Create(async () =>
            {
                await Task.CompletedTask.InAnyContext();

                throw exception;
            },
            CancellationToken.None);

        future.Wait().Switch(
            () => Assert.Fail(),
            e => Assert.Equal(exception, e)
        );
    }
}
