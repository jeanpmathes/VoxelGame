// <copyright file="MockLoadingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Tests.Utilities;

public class MockLoadingContext : ILoadingContext
{
    public IDisposable BeginStep(String name)
    {
        return new Disposer();
    }

    public void ReportSuccess(String type, String resource) {}

    public void ReportFailure(String type, String resource, Exception exception, Boolean abort = false) {}

    public void ReportFailure(String type, String resource, String message, Boolean abort = false) {}

    public void ReportWarning(String type, String resource, Exception exception) {}

    public void ReportWarning(String type, String resource, String message) {}
}
