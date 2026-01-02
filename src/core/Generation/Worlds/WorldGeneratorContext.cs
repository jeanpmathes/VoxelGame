// <copyright file="WorldGeneratorContext.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     Implementation of <see cref="IWorldGeneratorContext" />.
/// </summary>
public class WorldGeneratorContext(World world, Timer? timer) : IWorldGeneratorContext
{
    /// <inheritdoc />
    public (Int32 upper, Int32 lower) Seed => world.Seed;

    /// <inheritdoc />
    public Timer? Timer { get; } = timer;

    /// <inheritdoc />
    public T? ReadBlob<T>(String name) where T : class, IEntity, new()
    {
        return Operations.Launch(async token => await world.Data.ReadBlobAsync<T>(name, token).InAnyContext()).Wait().UnwrapWithFallback(() => null, out _);
    }

    /// <inheritdoc />
    public void WriteBlob<T>(String name, T entity) where T : class, IEntity, new()
    {
        Operations.Launch(async token => await world.Data.WriteBlobAsync(name, entity, token).InAnyContext()).Wait();
    }
}
