// <copyright file="SaltWaterFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Fluids;

/// <summary>
///     Evaporates, leaving salt behind.
/// </summary>
public class SaltWaterFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="SaltWaterFluid" />.
    /// </summary>
    public SaltWaterFluid(String name, String namedID, Single density, Int32 viscosity, TID texture) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint: true,
            texture,
            RenderType.Transparent) {}

    /// <inheritdoc />
    internal override void DoRandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic)
    {
        if (!isStatic || level != FluidLevel.One) return;
        if (world.GetBlock(position.Below())?.IsFullySolid != true) return;

        world.SetDefaultFluid(position);
        
        Blocks.Instance.Environment.Salt.Place(world, position); // todo: find a way to set the level of the concrete on placement, similar problem as with world gen and snow and such
    }
}
