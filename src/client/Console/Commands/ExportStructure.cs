// <copyright file="ExportStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Client.Utilities;
using VoxelGame.Core.Logic.Definitions.Structures;
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
    public override string Name => "export-structure";

    /// <inheritdoc />
    public override string HelpText => "Exports a structure to a file. Default content (Air, None) is ignored.";

    /// <exclude />
    public void Invoke(int x, int y, int z, int extentsX, int extentsY, int extentsZ, string name)
    {
        Export((x, y, z), (extentsX, extentsY, extentsZ), name);
    }

    /// <exclude />
    public void Invoke(int extentsX, int extentsY, int extentsZ, string name)
    {
        if (Context.Player.TargetPosition is {} targetPosition) Export(targetPosition, (extentsX, extentsY, extentsZ), name);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Export(Vector3i position, Vector3i extents, string name)
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

