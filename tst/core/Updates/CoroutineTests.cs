// <copyright file="CoroutineTests.cs" company="VoxelGame">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using VoxelGame.Core.App;
using VoxelGame.Core.Updates;
using Xunit;

namespace VoxelGame.Core.Tests.Updates;

[TestSubject(typeof(Coroutine))]
public sealed class CoroutineTests : IDisposable
{
    private readonly UpdateDispatch dispatch = new(singleton: false, Application.Instance);

    public void Dispose()
    {
        dispatch.Dispose();
    }

    [Fact]
    public void Coroutine_RunSimpleCoroutine_CoroutineRunsToCompletion()
    {
        var steps = 0;
        var disposed = false;

        Coroutine.Start(CoroutineBody, dispatch);

        dispatch.CompleteAll();

        Assert.Equal(expected: 3, steps);
        Assert.True(disposed);
        return;

        IEnumerable<Object?> CoroutineBody()
        {
            try
            {
                steps++;
                yield return null;

                steps++;
                yield return null;

                steps++;
                yield return null;
            }
            finally
            {
                disposed = true;
            }
        }
    }
}
