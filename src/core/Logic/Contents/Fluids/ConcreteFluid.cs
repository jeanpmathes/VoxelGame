// <copyright file="ConcreteFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Contents.Fluids;

/// <summary>
///     A concrete-like fluid that can harden to concrete blocks.
/// </summary>
public class ConcreteFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="ConcreteFluid" />.
    /// </summary>
    /// <param name="name">The name of the fluid.</param>
    /// <param name="namedID">The named ID of the fluid.</param>
    /// <param name="density">The density of the fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid.</param>
    /// <param name="texture">The texture of the fluid.</param>
    public ConcreteFluid(String name, String namedID, Density density, Viscosity viscosity, TID texture) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint: false,
            texture) {}

    /// <inheritdoc />
    internal override void DoRandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic)
    {
        if (!isStatic) return;
        if (!Blocks.Instance.Construction.Concrete.CanPlace(world, position)) return;

        world.SetDefaultFluid(position);
        
        State state = Blocks.Instance.Construction.Concrete.GetPlacementState(world, position);

        state = state.WithHeight(level.BlockHeight);

        world.SetBlock(state, position);
    }
}
