// <copyright file="PlayerDebugProperties.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Globalization;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Actors;

/// <summary>
///     Debug properties about the player and the world they are in.
/// </summary>
public class PlayerDebugProperties : Group
{
    /// <summary>
    ///     Create new debug properties for the player.
    /// </summary>
    /// <param name="player">The player to get the debug properties for.</param>
    public PlayerDebugProperties(Core.Actors.Player player) : base("Debug Data",
    [
        new Message("Position (Head)", FormatObject(player.Head?.Position.Floor())),
        new Message("Position (Target)", FormatObject(player.GetComponent<Targeting>()?.Position)),
        new Message("Position (Chunk)", FormatObject(player.GetComponent<ChunkLoader>()?.Chunk)),
        new Message("Position (Section)", FormatObject(SectionPosition.From(player.Body.Transform.Position.Floor()))),
        new Group("Target Block", CreateBlockTargetProperties(player.GetComponent<Targeting>()?.Block ?? Content.DefaultState)),
        new Group("Target Fluid", CreateFluidTargetProperties(player.GetComponent<Targeting>()?.Fluid ?? FluidInstance.Default)),
        new Measure("Temperature", player.World.Map.GetTemperature(player.Body.Transform.Position.Floor())),
        new Message("Date and Time", $"{player.World.DateAndTime}"),
        new Group("World",
        [
            new Message("Chunk State Updates", $"{player.World.ChunkStateUpdateCount}"),
            player.World.Map.GetPositionDebugData(player.Body.Transform.Position)
        ]),
        Profile.Instance?.GenerateReport() ?? new Group(nameof(Profile),
        [
            new Message("Disabled", "Use application arguments to enable integrated profiling.")
        ])
    ]) {}

    private static IEnumerable<Property> CreateBlockTargetProperties(State block)
    {
        yield return new Message("Block ID", $"{block.Block.ContentID}[{block.Block.BlockID}]");
        yield return new Message("State ID", $"{block.ID}");
        yield return new Message("State Index", $"{block.Index} of {block.Block.States.Count}");
        yield return new Group("Attributes", block.CreateProperties());
    }

    private static IEnumerable<Property> CreateFluidTargetProperties(FluidInstance instance)
    {
        yield return new Message("ID", $"{instance.Fluid.NamedID}[{instance.Fluid.ID}]");
        yield return new Message("Level", instance.Level.ToString());
        yield return new Message("IsStatic", instance.IsStatic.ToString(CultureInfo.InvariantCulture));
    }

    private static String FormatObject(Object? obj)
    {
        return obj != null ? $"{obj}" : "null";
    }
}
