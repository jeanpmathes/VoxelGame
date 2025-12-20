// <copyright file="DoUpdate.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Cause a random update to occur for a targeted position.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DoUpdate : Command
{
    /// <inheritdoc />
    public override String Name => "do-update";

    /// <inheritdoc />
    public override String HelpText => "Cause a 'random' update to occur for a targeted position.";

    /// <exclude />
    public void Invoke()
    {
        if (Context.Player.GetComponentOrThrow<Targeting>().Position is {} targetPosition) Update(targetPosition);
        else Context.Output.WriteError("No position targeted.");
    }

    /// <exclude />
    public void Invoke(Int32 x, Int32 y, Int32 z)
    {
        Update((x, y, z));
    }

    private void Update(Vector3i position)
    {
        Boolean success = Context.Player.World.DoRandomUpdate(position);
        if (!success) Context.Output.WriteError("Cannot update at this position.");
    }
}
