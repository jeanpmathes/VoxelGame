// <copyright file="SetFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Sets the fluid at the target position. Can cause invalid fluid state.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetFluid : Command
{
    /// <inheritdoc />
    public override string Name => "set-fluid";

    /// <inheritdoc />
    public override string HelpText => "Sets the fluid at the target position. Can cause invalid fluid state.";

    /// <exclude />
    public void Invoke(string namedID, int level, int x, int y, int z)
    {
        Set(namedID, level, (x, y, z));
    }

    /// <exclude />
    public void Invoke(string namedID, int level)
    {
        if (Context.Player.TargetPosition is {} targetPosition) Set(namedID, level, targetPosition);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Set(string namedID, int levelData, Vector3i position)
    {
        Fluid? fluid = Fluids.Instance.TranslateNamedID(namedID);

        if (fluid == null)
        {
            Context.Console.WriteError("Cannot find fluid.");

            return;
        }

        var level = (FluidLevel) levelData;

        if (level is < FluidLevel.One or > FluidLevel.Eight)
        {
            Context.Console.WriteError("Invalid level.");

            return;
        }

        Context.Player.World.SetFluid(fluid.AsInstance(level), position);
    }
}


