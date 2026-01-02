// <copyright file="SetTime.cs" company="VoxelGame">
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
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Set the time of day in the world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetTime : Command
{
    /// <inheritdoc />
    public override String Name => "set-time";

    /// <inheritdoc />
    public override String HelpText => "Set the time of day in the world.";

    /// <exclude />
    public void Invoke(Double timeOfDay)
    {
        if (timeOfDay is < 0.0 or >= 1.0)
        {
            Context.Output.WriteError("Time of day must be in the range [0.0, 1.0).");
        }
        else
        {
            Context.Player.World.TimeOfDay = timeOfDay;
        }
    }
}
