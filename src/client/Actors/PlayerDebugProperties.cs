// <copyright file="PlayerDebugProperties.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
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
        new Message("Target Block", FormatBlockTarget(player.GetComponent<Targeting>()?.Block ?? BlockInstance.Default)),
        new Message("Target Fluid", FormatFluidTarget(player.GetComponent<Targeting>()?.Fluid ?? FluidInstance.Default)),
        new Measure("Temperature", player.World.Map.GetTemperature(player.Body.Transform.Position)),
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

    private static String FormatBlockTarget(BlockInstance instance)
    {
        return $"{instance.Block.NamedID}[{instance.Block.ID}], {instance.Data:B}";
    }

    private static String FormatFluidTarget(FluidInstance instance)
    {
        return $"{instance.Fluid.NamedID}[{instance.Fluid.ID}], {instance.Level}, {instance.IsStatic}";
    }
    
    private static String FormatObject(Object? obj)
    {
        return obj != null 
            ? $"{obj.GetType().Name} ({obj})" 
            : "null";
    }
}
