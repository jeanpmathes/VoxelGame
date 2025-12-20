// <copyright file="IConvention.cs" company="VoxelGame">
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

using System.Collections.Generic;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A convention defines a group of content that is used in the game.
/// </summary>
public interface IConvention : IContent
{
    /// <summary>
    ///     The content that is part of this convention instance.
    /// </summary>
    IEnumerable<IContent> Content { get; }

    /// <inheritdoc />
    ResourceType IResource.Type => ResourceTypes.Convention;
}
