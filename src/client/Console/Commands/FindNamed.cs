// <copyright file="FindNamed.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Presentation.Legacy.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Search and find any named generated entity in the world.
/// </summary>
public class FindNamed : Command
{
    /// <inheritdoc />
    public override String Name => "find-named";

    /// <inheritdoc />
    public override String HelpText => "Search and find any named generated entity in the world.";

    /// <exclude />
    public void Invoke(String name, Context context)
    {
        Search(name, count: 1, World.BlockLimit * 2, context);
    }

    /// <exclude />
    public void Invoke(String name, Int32 count, Context context)
    {
        Search(name, count, World.BlockLimit * 2, context);
    }

    /// <exclude />
    public void Invoke(String name, Int32 count, UInt32 maxDistance, Context context)
    {
        Search(name, count, maxDistance, context);
    }

    private void Search(String name, Int32 count, UInt32 maxDistance, Context context)
    {
        if (count < 1)
        {
            context.Output.WriteError("Count must be greater than 0.");

            return;
        }

        IEnumerable<Vector3i>? positions = context.Player.World
            .SearchNamedGeneratedElements(context.Player.Body.Transform.Position.Floor(), name, maxDistance);

        if (positions == null)
        {
            context.Output.WriteError($"Search failed, name {name} not valid.");

            return;
        }

        context.Output.WriteResponse($"Beginning search for {count} {name} elements...");

        Operations.Launch(async token =>
        {
            foreach (Vector3i position in positions.Take(count))
                await context.Output.WriteResponseAsync($"Found {name} at {position}.",
                    [new FollowUp($"Teleport to {name}", () => Teleport.Do(context, position))],
                    token).InAnyContext();

            await context.Output.WriteResponseAsync($"Search for {name} finished.", [], token).InAnyContext();
        });
    }
}
