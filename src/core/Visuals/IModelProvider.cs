// <copyright file="IModelProvider.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides loaded models.
/// </summary>
public interface IModelProvider : IResourceProvider
{
    /// <summary>
    ///     Get the model for a given identifier.
    /// </summary>
    /// <param name="identifier">The resource identifier.</param>
    /// <param name="part">
    ///     The part of the model, if it is a model with a greater size than one block, or <c>null</c> to get
    ///     the full model.
    /// </param>
    /// <returns>The model, or a fallback model if the model is not found.</returns>
    Model GetModel(RID identifier, Vector3i? part = null);
}
