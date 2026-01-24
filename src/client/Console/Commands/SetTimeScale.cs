// <copyright file="SetTimeScale.cs" company="VoxelGame">
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
///     Set the speed of time progression.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetTimeScale : Command
{
    /// <inheritdoc />
    public override String Name => "set-timescale";

    /// <inheritdoc />
    public override String HelpText => "Sets how fast time progresses in the game.";

    /// <exclude />
    public void Invoke(Double timeScale)
    {
        if (timeScale <= 0)
        {
            Context.Output.WriteError("Time scale must be greater than zero.");

            return;
        }

        Core.App.Application.Instance.SetTimeScale(timeScale);
    }
}
