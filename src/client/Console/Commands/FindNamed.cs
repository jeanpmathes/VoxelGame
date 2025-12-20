// <copyright file="FindNamed.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Search and find any named generated entity in the world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FindNamed : Command
{
    /// <inheritdoc />
    public override String Name => "find-named";

    /// <inheritdoc />
    public override String HelpText => "Search and find any named generated entity in the world.";

    /// <exclude />
    public void Invoke(String name)
    {
        Search(name);
    }

    /// <exclude />
    public void Invoke(String name, Int32 count)
    {
        Search(name, count);
    }

    /// <exclude />
    public void Invoke(String name, Int32 count, UInt32 maxDistance)
    {
        Search(name, count, maxDistance);
    }

    private void Search(String name, Int32 count = 1, UInt32 maxDistance = World.BlockLimit * 2)
    {
        if (count < 1)
        {
            Context.Output.WriteError("Count must be greater than 0.");

            return;
        }

        IEnumerable<Vector3i>? positions = Context.Player.World
            .SearchNamedGeneratedElements(Context.Player.Body.Transform.Position.Floor(), name, maxDistance);

        if (positions == null)
        {
            Context.Output.WriteError($"Search failed, name {name} not valid.");

            return;
        }

        Context.Output.WriteResponse($"Beginning search for {count} {name} elements...");

        Operations.Launch(async token =>
        {
            foreach (Vector3i position in positions.Take(count))
                await Context.Output.WriteResponseAsync($"Found {name} at {position}.",
                    [new FollowUp($"Teleport to {name}", () => Teleport.Do(Context, this, position))],
                    token).InAnyContext();

            await Context.Output.WriteResponseAsync($"Search for {name} finished.", [], token).InAnyContext();
        });
    }
}
