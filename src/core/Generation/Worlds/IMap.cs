// <copyright file="IMap.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     A map defines global attributes of the entire world.
/// </summary>
public interface IMap
{
    /// <summary>
    ///     Get debug data for a given position, which is shown to the player in the debug view.
    /// </summary>
    /// <param name="position">The world position of the player.</param>
    /// <returns>The debug properties for the position.</returns>
    Property GetPositionDebugData(Vector3d position);

    /// <summary>
    ///     Get the tint for a position.
    /// </summary>
    (ColorS block, ColorS fluid) GetPositionTint(Vector3d position);

    /// <summary>
    ///     Get the temperature for a position.
    /// </summary>
    /// <param name="position">The position to get the temperature for.</param>
    /// <returns>The temperature, in degrees Celsius.</returns>
    Temperature GetTemperature(Vector3d position);
}
