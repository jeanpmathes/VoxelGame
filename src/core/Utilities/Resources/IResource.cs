// <copyright file="IResource.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     A resource can be loaded by a <see cref="IResourceLoader" />.
/// </summary>
public interface IResource : IDisposable
{
    /// <summary>
    ///     A justification string to suppress <c>CA2213</c> warnings.
    /// </summary>
    const String ResourcesOwnedByContext = "Resources are owned by the context and should not be disposed manually.";

    /// <summary>
    ///     An identifier for the resource.
    /// </summary>
    RID Identifier { get; }

    /// <summary>
    ///     The type of this resource.
    /// </summary>
    ResourceType Type { get; }

    /// <summary>
    ///     Get an error that occurred during loading, or <c>null</c> if no error occurred.
    ///     If this is set, the resource is considered invalid and will not be added to the context.
    /// </summary>
    ResourceIssue? Issue => null;
}
