// <copyright file="SetOverlays.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Set whether to draw the block/fluid-in-head overlays.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetOverlays : Command
{
    /// <inheritdoc />
    public override String Name => "set-overlays";

    /// <inheritdoc />
    public override String HelpText => "Set whether the fluid/block overlays are enabled.";

    /// <exclude />
    public void Invoke(Boolean enabled)
    {
        Do(Context, enabled);
    }

    /// <summary>
    ///     Externally simulate a command invocation, setting the overlay state.
    /// </summary>
    public static void Do(Context context, Boolean enabled)
    {
        if (context.Player.GetComponent<OverlayDisplay>() is {} overlay)
            overlay.IsEnabled = enabled;
    }
}
