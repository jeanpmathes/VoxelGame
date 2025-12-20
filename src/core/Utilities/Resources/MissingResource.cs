// <copyright file="ErrorResource.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     Proxy resource that represents resource that could not be loaded.
/// </summary>
/// <param name="type">The type of the resource.</param>
/// <param name="identifier">The identifier of the resource.</param>
/// <param name="issue">The error that occurred during loading.</param>
public sealed class MissingResource(ResourceType type, RID identifier, ResourceIssue issue) : IResource
{
    /// <inheritdoc />
    public RID Identifier => identifier;

    /// <inheritdoc />
    public ResourceType Type => type;

    /// <inheritdoc />
    public ResourceIssue Issue => issue;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE
}
