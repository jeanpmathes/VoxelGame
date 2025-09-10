// <copyright file="MockResourceContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Tests.Utilities.Resources;

#pragma warning disable CS0067 // Is for mock purposes only, so unused events do not matter.

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class MockResourceContext : IResourceContext
{
    public IEnumerable<IResource> Require<T>(Func<T, IEnumerable<IResource>> func) where T : class
    {
        return [];
    }

    public IEnumerable<IResource> Require<T>(RID identifier, Func<T, IEnumerable<IResource>> func) where T : class
    {
        return [];
    }

    public T? Get<T>(RID? identifier = null) where T : class
    {
        return null;
    }

    public IEnumerable<T> GetAll<T>() where T : class
    {
        return [];
    }

    public void ReportWarning(IIssueSource source, String message, Exception? exception = null, FileSystemInfo? path = null) {}
    public void ReportError(IIssueSource source, String message, Exception? exception = null, FileSystemInfo? path = null) {}
    public void ReportDiscovery(ResourceType type, RID identifier, Exception? error = null, String? errorMessage = null) {}
    public event EventHandler? Completed;
    public void Dispose() {}
}
