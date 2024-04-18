// <copyright file="DebugProperties.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Collections.Properties;
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
    public DebugProperties(Player player) : base("Debug Data",
    [
        new Message("Position (Head/Target)", $"{player.Head.Position.Floor()}/{player.TargetPosition}"),
        new Message("Target Block", $"{player.TargetBlock.Block.NamedID}[{player.TargetBlock.Block.ID}], {player.TargetBlock.Data:B}"),
        new Message("Target Fluid", $"{player.TargetFluid.Fluid.NamedID}[{player.TargetFluid.Fluid.ID}], {player.TargetFluid.Level}, {player.TargetFluid.IsStatic}"),
        new Measure("Temperature", player.World.Map.GetTemperature(player.Position)),
        player.World.Map.GetPositionDebugData(player.Position),
        Profile.Instance?.GenerateReport() ?? new Group(nameof(Profile),
        [
            new Message("Disabled", "Use application arguments to enable integrated profiling.")
        ])
    ]) {}
}
