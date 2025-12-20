// <copyright file="IStuffer.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     Stuffer is used when a local negative offset is applied to the terrain.
///     The stuffer stuffs the space between the global height and the local height.
/// </summary>
public interface IStuffer
{
    /// <summary>
    ///     Get the content of this stuffer for a position.
    /// </summary>
    /// <param name="temperature">The temperature of the position.</param>
    /// <returns>The content of the stuffer.</returns>
    Content GetContent(Temperature temperature);

    /// <summary>
    ///     Simply stuffs with ice.
    /// </summary>
    sealed class Ice : IStuffer
    {
        private readonly Content content = new(Blocks.Instance.Environment.Ice.States.GenerationDefault.WithHeight(BlockHeight.Maximum), FluidInstance.Default);

        /// <inheritdoc />
        public Content GetContent(Temperature temperature)
        {
            return content;
        }
    }

    /// <summary>
    ///     Stuffs with water, or ice if the temperature is low.
    /// </summary>
    sealed class Water : IStuffer
    {
        private readonly Content ice = new(Blocks.Instance.Environment.Ice.States.GenerationDefault.WithHeight(BlockHeight.Maximum), FluidInstance.Default);
        private readonly Content water = new(Content.DefaultState, Fluids.Instance.FreshWater.AsInstance());

        /// <inheritdoc />
        public Content GetContent(Temperature temperature)
        {
            return temperature.IsFreezing ? ice : water;
        }
    }
}
