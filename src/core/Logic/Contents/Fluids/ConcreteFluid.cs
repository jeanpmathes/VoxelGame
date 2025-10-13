// <copyright file="ConcreteFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
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
    public ConcreteFluid(String name, String namedID, Single density, Int32 viscosity, TID texture) :
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

        world.SetDefaultFluid(position);

        Blocks.Instance.Construction.Concrete.Place(world, position); 
        
        // todo: find a way to set the level of the concrete on placement, similar problem as with world gen and snow and such
        // todo: easiest method: a behavior extensions static class, with utilities to do this
        // todo: for height, add a stored height behavior that is required by both height 16 and 8 and has method to get appropriate state
    }
}
