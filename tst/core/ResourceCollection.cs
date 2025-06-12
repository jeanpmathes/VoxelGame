// <copyright file="ResourceCollection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Resources;
using VoxelGame.Core.Utilities.Resources;
using Xunit;

namespace VoxelGame.Core.Tests;

[UsedImplicitly]
public sealed class ResourceFixture : DispatcherFixture
{
    private readonly IResourceContext resources;

    public ResourceFixture()
    {
        ResourceCatalogLoader loader = new();

        (resources, _) = loader.Load(new CoreContent(), timer: null);
    }

    protected override void Dispose(Boolean disposing)
    {
        if (disposing)
            resources.Dispose();
    }
}

[CollectionDefinition(Name)]
public class ResourceCollection : ICollectionFixture<ResourceFixture>
{
    public const String Name = "RequireResources";
}
