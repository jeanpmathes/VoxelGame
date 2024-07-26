// <copyright file="SetFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Sets the fluid at the target position. Can cause invalid fluid state.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetFluid : Command
{
    /// <inheritdoc />
    public override String Name => "set-fluid";

    /// <inheritdoc />
    public override String HelpText => "Sets the fluid at the target position. Can cause invalid fluid state.";

    /// <exclude />
    public void Invoke(String namedID, Int32 level, Int32 x, Int32 y, Int32 z)
    {
        Set(namedID, level, (x, y, z));
    }

    /// <exclude />
    public void Invoke(String namedID, Int32 level)
    {
        if (Context.Player.TargetPosition is {} targetPosition) Set(namedID, level, targetPosition);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Set(String namedID, Int32 levelData, Vector3i position)
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
