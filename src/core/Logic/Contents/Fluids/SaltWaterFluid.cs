// <copyright file="SaltWaterFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Contents.Fluids;

/// <summary>
///     Evaporates, leaving salt behind.
/// </summary>
public class SaltWaterFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="SaltWaterFluid" />.
    /// </summary>
    public SaltWaterFluid(String name, String namedID, Density density, Viscosity viscosity, TID texture) :
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
        
        if (!Blocks.Instance.Environment.Salt.CanPlace(world, position)) return;

        State state = Blocks.Instance.Environment.Salt.GetPlacementState(world, position);
        
        state = state.WithHeight(level.GetBlockHeight());
        
        world.SetBlock(state, position);
    }
}
