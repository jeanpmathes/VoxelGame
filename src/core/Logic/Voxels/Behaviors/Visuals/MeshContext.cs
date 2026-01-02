// <copyright file="MeshContext.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Provides the context required to generate meshes for blocks.
/// </summary>
/// <param name="State">The state of the block for which the mesh is generated.</param>
/// <param name="TextureIndexProvider">Provides texture indices used during mesh generation.</param>
/// <param name="ModelProvider">Provides models used during mesh generation.</param>
/// <param name="Validator">Validator to check for validity during mesh generation.</param>
public readonly record struct MeshContext(
    State State,
    ITextureIndexProvider TextureIndexProvider,
    IModelProvider ModelProvider,
    IValidator Validator);
