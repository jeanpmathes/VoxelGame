// <copyright file="ResourceCatalogLoaderTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities.Resources;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities.Resources;

[Collection(LoggerCollection.Name)]
[TestSubject(typeof(ResourceCatalogLoader))]
public class ResourceCatalogLoaderTests
{
    [Fact]
    public void ResourceCatalogLoader_ShouldLoadAllEntriesInCatalog()
    {
        ResourceCatalogLoader loader = new();
        CallTracker tracker = new();

        loader.AddToEnvironment(tracker);
        (_, ResourceLoadingIssueReport? report) = loader.Load(new MockCatalog(), timer: null);

        Assert.True(tracker.LoaderCalled);
        Assert.True(tracker.LinkerCalled);
        Assert.True(tracker.ProviderCalled);

        Assert.NotNull(report);
        Assert.Equal(expected: 3, report.WarningCount);
        Assert.Equal(expected: 1, report.ErrorCount);
    }

    private sealed class CallTracker
    {
        public Boolean LoaderCalled { get; set; }

        public Boolean LinkerCalled { get; set; }

        public Boolean ProviderCalled { get; set; }
    }

    private sealed class MockCatalog() : ResourceCatalog([
        new MockLoader(),
        new MockLinker(),
        new MockProvider()
    ]);

    private sealed class MockLoader : IResourceLoader
    {
        public String Instance => "";

        public IEnumerable<IResource> Load(IResourceContext context)
        {
            return context.Require<CallTracker>(tracker =>
            {
                tracker.LoaderCalled = true;

                context.ReportWarning(this, "");
                context.ReportWarning(this, "");
                context.ReportWarning(this, "");

                // Cause an error:
                context.Require<MockResource>(_ => []);

                return [new MockResource()];
            });
        }
    }

    private sealed class MockLinker : IResourceLinker
    {
        public String Instance => "";

        public void Link(IResourceContext context)
        {
            context.Require<CallTracker>(tracker =>
            {
                tracker.LinkerCalled = true;

                return [];
            });
        }
    }

    private sealed class MockProvider : IResourceProvider
    {
        public IResourceContext? Context { get; set; }
        public String Instance => "";

        public void SetUp()
        {
            Context?.Require<CallTracker>(tracker =>
            {
                tracker.ProviderCalled = true;

                return [];
            });
        }
    }

    private sealed class MockResource : IResource
    {
        public RID Identifier { get; } = RID.Named<MockResource>("Default");

        public ResourceType Type { get; } = new(ResourceType.Category.Meta, "mock");

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
