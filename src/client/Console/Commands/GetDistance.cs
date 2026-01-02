// <copyright file="GetDistance.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Get the distance to a specified point or target.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GetDistance : Command
{
    /// <inheritdoc />
    public override String Name => "get-distance";

    /// <inheritdoc />
    public override String HelpText => "Get the distance to a specified point or target.";

    /// <exclude />
    public void Invoke(Double x, Double y, Double z)
    {
        DetermineDistance((x, y, z));
    }

    /// <exclude />
    public void Invoke(String target)
    {
        if (GetNamedPosition(target) is {} position) DetermineDistance(position);
        else Context.Output.WriteError($"Unknown target: {target}");
    }

    private void DetermineDistance(Vector3d position)
    {
        Length distance = new()
        {
            Meters = (position - Context.Player.Body.Transform.Position).Length
        };

        Context.Output.WriteResponse($"Distance: {distance}");
    }
}
