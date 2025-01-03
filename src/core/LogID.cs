// <copyright file="LogID.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

    internal const UInt16 BlockModel = Events.Increment + DefaultMap;

    internal const UInt16 OS = Events.Increment + BlockModel;

    internal const UInt16 FileSystem = Events.Increment + OS;

    internal const UInt16 ResourceLoader = Events.Increment + FileSystem;

    internal const UInt16 GroupProvider = Events.Increment + ResourceLoader;
}
