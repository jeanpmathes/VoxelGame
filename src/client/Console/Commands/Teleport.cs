// <copyright file="Teleport.cs" company="VoxelGame">
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
using OpenTK.Mathematics;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Teleport to a specified position or target.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Teleport : Command
{
    /// <inheritdoc />
    public override String Name => "teleport";

    /// <inheritdoc />
    public override String HelpText => "Teleport to a specified position or target.";

    /// <exclude />
    public void Invoke(Double x, Double y, Double z)
    {
        Do(Context, this, (x, y, z));
    }

    /// <exclude />
    public void Invoke(String target)
    {
        if (GetNamedPosition(target) is {} position) Do(Context, this, position);
        else Context.Output.WriteError($"Unknown target: {target}");
    }

    /// <summary>
    ///     Externally simulate command invocation, teleporting the player.
    /// </summary>
    /// <param name="context">The context in which the command is executed.</param>
    /// <param name="command">The command being invoked.</param>
    /// <param name="position">The position to teleport to.</param>
    public static void Do(Context context, Command command, Vector3d position)
    {
        command.SetPreviousPlayerPosition(context.Player.Body.Transform.Position);
        context.Player.Body.Transform.Position = position;
    }
}
