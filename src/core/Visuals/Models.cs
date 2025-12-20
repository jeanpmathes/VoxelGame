// <copyright file="Models.cs" company="VoxelGame">
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

using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Utilities for <see cref="Model" />s.
/// </summary>
public static class Models
{
    /// <summary>
    ///     Create models for all four horizontal orientations.
    /// </summary>
    /// <param name="model">The base model, will be used for the north orientation.</param>
    /// <param name="mode">The transformation mode to use when creating the rotated models.</param>
    /// <returns>A tuple containing the models for the north, east, south and west orientations.</returns>
    public static (Model north, Model east, Model south, Model west) CreateModelsForAllOrientations(Model model, Model.TransformationMode mode = Model.TransformationMode.Rotate)
    {
        return (
            model,
            model.CreateModelForOrientation(Orientation.East, mode),
            model.CreateModelForOrientation(Orientation.South, mode),
            model.CreateModelForOrientation(Orientation.West, mode)
        );
    }

    /// <summary>
    ///     Create models for all six sides.
    /// </summary>
    /// <param name="model">The base model, will be used for the front side.</param>
    /// <param name="mode">The transformation mode to use when creating the models for the other sides.</param>
    /// <returns>A tuple containing the models for the front, back, left, right, bottom and top sides.</returns>
    public static (Model front, Model back, Model left, Model right, Model bottom, Model top) CreateModelsForAllSides(Model model, Model.TransformationMode mode = Model.TransformationMode.Rotate)
    {
        return (
            model,
            model.CreateModelForSide(Side.Back, mode),
            model.CreateModelForSide(Side.Left, mode),
            model.CreateModelForSide(Side.Right, mode),
            model.CreateModelForSide(Side.Bottom, mode),
            model.CreateModelForSide(Side.Top, mode)
        );
    }
}
