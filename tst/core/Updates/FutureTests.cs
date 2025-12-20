// <copyright file="FutureTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
