// <copyright file="ImportStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Structures;

namespace VoxelGame.Client.Console.Commands;
#pragma warning disable CA1822

/// <summary>
///     Import a structure from a file.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ImportStructure : Command
{
    /// <inheritdoc />
    public override string Name => "import-structure";

    /// <inheritdoc />
    public override string HelpText => "Imports a structure from a file.";

    /// <exclude />
    public void Invoke(int x, int y, int z, string name)
    {
        Import((x, y, z), name);
    }

    /// <exclude />
    public void Invoke(string name)
    {
        if (Context.Player.TargetPosition is {} targetPosition) Import(targetPosition, name);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Import(Vector3i position, string name)
    {
        StaticStructure structure = StaticStructure.Load(Program.StructureDirectory, name);
        structure.Place(Context.Player.World, position);
    }
}
