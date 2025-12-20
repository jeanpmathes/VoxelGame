// <copyright file="Map.cs" company="VoxelGame">
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
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Worlds.Water;

/// <summary>
///     The map for the water world.
/// </summary>
public class Map : IMap
{
    /// <inheritdoc />
    public Property GetPositionDebugData(Vector3d position)
    {
        return new Message(nameof(Water), "No special information.");
    }

    /// <inheritdoc />
    public (ColorS block, ColorS fluid) GetPositionTint(Vector3d position)
    {
        return (ColorS.Green, ColorS.Blue);
    }

    /// <inheritdoc />
    public Temperature GetTemperature(Vector3d position)
    {
        return new Temperature {DegreesCelsius = 20};
    }
}
