// <copyright file="EditScript.cs" company="VoxelGame">
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
using System.IO;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Open a script for editing.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EditScript : Command
{
    /// <inheritdoc />
    public override String Name => "edit-script";

    /// <inheritdoc />
    public override String HelpText => "Edit a ready script by opening it.";

    /// <exclude />
    public void Invoke(String name)
    {
        Do(Context, name);
    }

    /// <summary>
    ///     Perform a run of the command, allowing to edit a script.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <param name="name">The name of the script to edit.</param>
    public static void Do(Context context, String name)
    {
        FileInfo? path = context.Player.World.Data.CreateScript(name, "");

        if (path == null) return;

        OS.Start(path);
    }
}
