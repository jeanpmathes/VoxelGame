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
public sealed class LoggerFixture : IDisposable
{
    public LoggerFixture()
    {
        LoggingHelper.SetupMockLogging();
    }

    public void Dispose()
    {
        // Nothing to do here.
    }
}

[CollectionDefinition("Logger")]
public class LoggerCollection : ICollectionFixture<LoggerFixture>
{
    // Nothing to do here.
}
