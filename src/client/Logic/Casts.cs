// <copyright file="Casts.cs" company="VoxelGame">
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

using VoxelGame.Client.Logic.Chunks;
using VoxelGame.Client.Logic.Sections;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Provides utility extensions to simplify casts to client classes.
/// </summary>
public static class Casts
{
    /// <summary>
    ///     Cast a client world.
    /// </summary>
    public static World Cast(this Core.Logic.World chunk)
    {
        return (World) chunk;
    }

    /// <summary>
    ///     Cast a client chunk.
    /// </summary>
    public static Chunk Cast(this Core.Logic.Chunks.Chunk chunk)
    {
        return (Chunk) chunk;
    }

    /// <summary>
    ///     Cast a client section.
    /// </summary>
    public static Section Cast(this Core.Logic.Sections.Section section)
    {
        return (Section) section;
    }
}
