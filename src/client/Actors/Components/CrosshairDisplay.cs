// <copyright file="CrosshairDisplay.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Displays the crosshair for the player.
/// </summary>
public partial class CrosshairDisplay : ActorComponent
{
    private readonly Engine engine;

    [Constructible]
    private CrosshairDisplay(Player player, Engine engine) : base(player)
    {
        this.engine = engine;
    }

    /// <inheritdoc />
    public override void OnActivate()
    {
        engine.CrosshairPipeline.IsEnabled = true;
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        engine.CrosshairPipeline.IsEnabled = false;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Delta delta)
    {
        engine.CrosshairPipeline.LogicUpdate();
    }
}
