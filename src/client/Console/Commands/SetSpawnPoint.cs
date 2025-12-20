// <copyright file="SetSpawnPoint.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Sets the spawn position for the current world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetSpawnPoint : Command
{
    /// <inheritdoc />
    public override String Name => "set-spawnpoint";

    /// <inheritdoc />
    public override String HelpText => "Sets the spawn position for the current world.";

    /// <exclude />
    public void Invoke(Double x, Double y, Double z)
    {
        SetSpawnPosition((x, y, z));
    }

    /// <exclude />
    public void Invoke()
    {
        SetSpawnPosition(Context.Player.Body.Transform.Position);
    }

    private void SetSpawnPosition(Vector3d newSpawnPoint)
    {
        Context.Player.World.SpawnPosition = newSpawnPoint;
    }
}
