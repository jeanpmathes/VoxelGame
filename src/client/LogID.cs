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

namespace VoxelGame.Client;

/// <summary>
///     Defines the logging event IDs for this project.
/// </summary>
internal static class LogID
{
    internal const UInt16 Program = Events.ClientID;

    internal const UInt16 Client = Events.Increment + Program;

    internal const UInt16 Arguments = Events.Increment + Client;

    internal const UInt16 ManualBuilder = Events.Increment + Arguments;

    internal const UInt16 Settings = Events.Increment + ManualBuilder;

    internal const UInt16 WorldMetadata = Events.Increment + Settings;

    internal const UInt16 WorldProvider = Events.Increment + WorldMetadata;

    internal const UInt16 Player = Events.Increment + WorldProvider;

    internal const UInt16 CommandInvoker = Events.Increment + Player;

    internal const UInt16 GameConsole = Events.Increment + CommandInvoker;

    internal const UInt16 KeybindManager = Events.Increment + GameConsole;

    internal const UInt16 Chunk = Events.Increment + KeybindManager;

    internal const UInt16 ChunkStates = Events.Increment + Chunk;

    internal const UInt16 SessionScene = Events.Increment + ChunkStates;

    internal const UInt16 StartScene = Events.Increment + SessionScene;

    internal const UInt16 SceneFactory = Events.Increment + StartScene;

    internal const UInt16 SceneManager = Events.Increment + SceneFactory;

    internal const UInt16 Graphics = Events.Increment + SceneManager;

    internal const UInt16 TextureIndexProvider = Events.Increment + Graphics;

    internal const UInt16 IntermediateBundle = Events.Increment + TextureIndexProvider;

    internal const UInt16 GeneralSettings = Events.Increment + IntermediateBundle;
}
