// <copyright file="ImportStructure.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Import a structure from a file.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ImportStructure : Command
{
    /// <inheritdoc />
    public override String Name => "import-structure";

    /// <inheritdoc />
    public override String HelpText => "Imports a structure from a file.";

    /// <exclude />
    public void Invoke(Int32 x, Int32 y, Int32 z, String name)
    {
        Import((x, y, z), name, Orientation.North);
    }

    /// <exclude />
    public void Invoke(Int32 x, Int32 y, Int32 z, String name, Orientation orientation)
    {
        Import((x, y, z), name, orientation);
    }

    /// <exclude />
    public void Invoke(String name)
    {
        ImportAtTarget(name, Orientation.North);
    }

    /// <exclude />
    public void Invoke(String name, Orientation orientation)
    {
        ImportAtTarget(name, orientation);
    }

    private void ImportAtTarget(String name, Orientation orientation)
    {
        if (Context.Player.GetComponentOrThrow<Targeting>().Position is {} targetPosition) Import(targetPosition, name, orientation);
        else Context.Output.WriteError("No position targeted.");
    }

    private void Import(Vector3i position, String name, Orientation orientation)
    {
        Operations.Launch(async token => await StaticStructure.LoadSafelyAsync(Program.StructureDirectory, name, token).InAnyContext())
            .OnSuccessfulSync(result =>
            {
                result.Place(seed: 0, Context.Player.World, position, orientation);
            });
    }
}
