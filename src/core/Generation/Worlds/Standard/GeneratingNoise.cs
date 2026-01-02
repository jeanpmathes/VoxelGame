// <copyright file="GeneratingNoise.cs" company="VoxelGame">
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

using System;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     The noise generators used for generating the map.
/// </summary>
public sealed class GeneratingNoise : IDisposable
{
    /// <summary>
    ///     Create all noise generators used for the map generation.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    public GeneratingNoise(NoiseFactory factory)
    {
        Pieces = factory.CreateNext()
            .WithType(NoiseType.CellularNoise)
            .WithFrequency(frequency: 0.025f)
            .Build();

        Stone = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(frequency: 0.03f)
            .Build();
    }

    /// <summary>
    ///     The noise used for creating map pieces, which are the parts that make up continents.
    /// </summary>
    public NoiseGenerator Pieces { get; }

    /// <summary>
    ///     The noise used to determine the stone types of the cells.
    /// </summary>
    public NoiseGenerator Stone { get; }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        Pieces.Dispose();
        Stone.Dispose();
    }

    #endregion
}
