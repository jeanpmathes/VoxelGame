// <copyright file="NoiseFactory.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     Creates noise generators.
/// </summary>
public class NoiseFactory
{
    private readonly Random random;

    /// <summary>
    ///     Creates a new noise factory with an initial seed.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    public NoiseFactory(Int32 seed)
    {
        random = new Random(seed);
    }


    /// <summary>
    ///     Get the next noise builder.
    /// </summary>
    /// <returns>A new noise builder, using a different seed.</returns>
    public NoiseBuilder CreateNext()
    {
        return NoiseBuilder.Create(random.Next(Int32.MinValue, Int32.MaxValue));
    }
}
