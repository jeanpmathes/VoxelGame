// <copyright file="MockResourceContext.cs" company="VoxelGame">
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
