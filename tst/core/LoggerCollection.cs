// <copyright file="LoggerCollection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
