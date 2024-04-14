// <copyright file="ImportStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console.Commands;
#pragma warning disable CA1822

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
        if (Context.Player.TargetPosition is {} targetPosition) Import(targetPosition, name, orientation);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Import(Vector3i position, String name, Orientation orientation)
    {
        StaticStructure structure = StaticStructure.Load(Program.StructureDirectory, name);
        structure.Place(Context.Player.World, position, orientation);
    }
}
