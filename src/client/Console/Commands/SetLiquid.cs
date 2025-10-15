// <copyright file="SetFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Client.Console.Commands;

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
        if (Context.Player.GetComponentOrThrow<Targeting>().Position is {} targetPosition) Set(namedID, level, targetPosition);
        else Context.Output.WriteError("No position targeted.");
    }

    private void Set(String namedID, Int32 levelData, Vector3i position)
    {
        Fluid? fluid = Fluids.Instance.TranslateNamedID(namedID);

        if (fluid == null)
        {
            Context.Output.WriteError("Cannot find fluid.");

            return;
        }

        if (!FluidLevel.TryFromInt32(levelData, out FluidLevel level))
        {
            Context.Output.WriteError("Invalid level.");

            return;
        }

        Context.Player.World.SetFluid(fluid.AsInstance(level), position);
    }
}
