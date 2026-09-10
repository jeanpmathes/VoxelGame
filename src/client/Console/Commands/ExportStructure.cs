// <copyright file="ExportStructure.cs" company="VoxelGame">
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
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Presentation.Legacy.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Export a structure to a file.
/// </summary>
public class ExportStructure : Command
{
    /// <inheritdoc />
    public override String Name => "export-structure";

    /// <inheritdoc />
    public override String HelpText => "Exports a structure to a file. Default content (Air, None) is ignored.";

    /// <exclude />
    public void Invoke(Int32 x, Int32 y, Int32 z, Int32 extentsX, Int32 extentsY, Int32 extentsZ, String name, Context context)
    {
        Export((x, y, z), (extentsX, extentsY, extentsZ), name, context);
    }

    /// <exclude />
    public void Invoke(Int32 extentsX, Int32 extentsY, Int32 extentsZ, String name, Context context)
    {
        if (context.Player.GetComponentOrThrow<Targeting>().Position is {} targetPosition) Export(targetPosition, (extentsX, extentsY, extentsZ), name, context);
        else context.Output.WriteError("No position targeted.");
    }

    private static void Export(Vector3i position, Vector3i extents, String name, Context context)
    {
        StaticStructure? structure = StaticStructure.Read(context.Player.World, position, extents);

        Operations.Launch(async token =>
        {
            await ExportAsync(structure, name, context, token).InAnyContext();
        });
    }

    private static async Task ExportAsync(StaticStructure? structure, String name, Context context, CancellationToken token = default)
    {
        Boolean success = false;

        if (structure != null)
        {
            Result result = await structure.SaveAsync(Program.StructureDirectory, name, token).InAnyContext();

            success = result.Switch(
                () => true,
                _ => false);
        }

        if (success)
            await context.Output.WriteResponseAsync($"Structure exported to: {Program.StructureDirectory}",
                [new FollowUp("Open directory", () => { OS.Start(Program.StructureDirectory); })],
                token).InAnyContext();
        else await context.Output.WriteErrorAsync("Failed to export structure.", [], token).InAnyContext();
    }
}
