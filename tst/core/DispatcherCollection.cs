// <copyright file="DispatcherCollection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Updates;
using Xunit;

namespace VoxelGame.Core.Tests;

[UsedImplicitly]
public class DispatcherFixture : LoggerFixture
{
    public DispatcherFixture()
    {
        OperationUpdateDispatch.SetUpMockInstance();
    }
}

[CollectionDefinition(Name)]
public class DispatcherCollection : ICollectionFixture<DispatcherFixture>
{
    // Nothing to do here.

    public const String Name = "RequireDispatcher";
}
