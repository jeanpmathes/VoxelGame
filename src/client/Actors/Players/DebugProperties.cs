// <copyright file="DebugProperties.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Debug properties about the player and the world they are in.
/// </summary>
public class DebugProperties : Group
{
    /// <summary>
    ///     Create new debug properties for the player.
    /// </summary>
    /// <param name="player">The player to get the debug properties for.</param>
    /// <param name="targeting">The targeting system of the player.</param>
    public DebugProperties(PhysicsActor player, Targeting targeting) : base("Debug Data",
    [
        new Message("Position (Head/Target)", $"{player.Head.Position.Floor()}/{player.TargetPosition}"),
        new Message("Target Block", FormatBlockTarget(targeting.Block ?? BlockInstance.Default)),
        new Message("Target Fluid", FormatFluidTarget(targeting.Fluid ?? FluidInstance.Default)),
        new Measure("Temperature", player.World.Map.GetTemperature(player.Position)),
        player.World.Map.GetPositionDebugData(player.Position),
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
}
