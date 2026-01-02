// <copyright file="EmitWorldInfo.cs" company="VoxelGame">
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
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Emit information about the generated world for debugging.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class EmitWorldInfo : Command
{
    /// <inheritdoc />
    public override String Name => "emit-world-info";

    /// <inheritdoc />
    public override String HelpText => "Emit information about the generated world for debugging.";

    /// <exclude />
    public void Invoke()
    {
        DirectoryInfo path = Context.Player.World.Data.DebugDirectory;

        Context.Player.World.EmitWorldInfo(path).OnSuccessfulSync(() =>
        {
            Context.Output.WriteResponse($"Emitted world info to: {path}",
                [new FollowUp("Open folder", () => OS.Start(path))]);
        });
    }
}
