// <copyright file="LogID.cs" company="VoxelGame">
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
using VoxelGame.Logging;

namespace VoxelGame.Core;

/// <summary>
///     Defines the logging event IDs for this project.
/// </summary>
internal static class LogID
{
    internal const UInt16 World = Events.CoreID;

    internal const UInt16 WorldData = Events.Increment + World;

    internal const UInt16 WorldInformation = Events.Increment + WorldData;

    internal const UInt16 WorldState = Events.Increment + WorldInformation;

    internal const UInt16 Chunk = Events.Increment + WorldState;

    internal const UInt16 ChunkStates = Events.Increment + Chunk;

    internal const UInt16 ChunkSet = Events.Increment + ChunkStates;

    internal const UInt16 StaticStructure = Events.Increment + ChunkSet;

    internal const UInt16 Blocks = Events.Increment + StaticStructure;

    internal const UInt16 Fluids = Events.Increment + Blocks;

    internal const UInt16 Profile = Events.Increment + Fluids;

    internal const UInt16 ScheduledUpdateManager = Events.Increment + Profile;

    internal const UInt16 PhysicsActor = Events.Increment + ScheduledUpdateManager;

    internal const UInt16 DefaultGenerator = Events.Increment + PhysicsActor;

    internal const UInt16 DefaultMap = Events.Increment + DefaultGenerator;

    internal const UInt16 Model = Events.Increment + DefaultMap;

    internal const UInt16 OS = Events.Increment + Model;

    internal const UInt16 FileSystem = Events.Increment + OS;

    internal const UInt16 ResourceLoader = Events.Increment + FileSystem;

    internal const UInt16 GroupProvider = Events.Increment + ResourceLoader;
}
