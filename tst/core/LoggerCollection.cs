// <copyright file="LoggerCollection.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.Logging;
using Xunit;

namespace VoxelGame.Core.Tests;

[UsedImplicitly]
public class LoggerFixture : IDisposable
{
    public LoggerFixture()
    {
        LoggingHelper.SetUpMockLogging();
    }

    #region DISPOSABLE

    protected virtual void Dispose(Boolean disposing) {}

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~LoggerFixture()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}

[CollectionDefinition(Name)]
public class LoggerCollection : ICollectionFixture<LoggerFixture>
{
    // Nothing to do here.

    public const String Name = "RequireLogger";
}
