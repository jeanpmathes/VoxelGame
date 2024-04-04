// <copyright file="ExportStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;
#pragma warning disable CA1822

/// <summary>
///     Export a structure to a file.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ExportStructure : Command
{
    /// <inheritdoc />
    public override String Name => "export-structure";

    /// <inheritdoc />
    public override String HelpText => "Exports a structure to a file. Default content (Air, None) is ignored.";

    /// <exclude />
    public void Invoke(Int32 x, Int32 y, Int32 z, Int32 extentsX, Int32 extentsY, Int32 extentsZ, String name)
    {
        Export((x, y, z), (extentsX, extentsY, extentsZ), name);
    }

    /// <exclude />
    public void Invoke(Int32 extentsX, Int32 extentsY, Int32 extentsZ, String name)
    {
        if (Context.Player.TargetPosition is {} targetPosition) Export(targetPosition, (extentsX, extentsY, extentsZ), name);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Export(Vector3i position, Vector3i extents, String name)
    {
        StaticStructure? structure = StaticStructure.Read(Context.Player.World, position, extents);

        var success = false;

        if (structure != null) success = structure.Store(Program.StructureDirectory, name);

        if (success)
            Context.Console.WriteResponse($"Structure exported to: {Program.StructureDirectory}",
                new FollowUp("Open directory", () => { OS.Start(Program.StructureDirectory); }));
        else Context.Console.WriteError("Failed to export structure.");
    }
}
