// <copyright file="ConcreteFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Fluids;

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
    /// <param name="movingLayout">The texture layout when this fluid is moving.</param>
    /// <param name="staticLayout">The texture layout when this fluid is static.</param>
    public ConcreteFluid(String name, String namedID, Single density, Int32 viscosity, TextureLayout movingLayout,
        TextureLayout staticLayout) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint: false,
            movingLayout,
            staticLayout) {}

    /// <inheritdoc />
    internal override void RandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic)
    {
        if (!isStatic) return;

        world.SetDefaultFluid(position);
        Elements.Blocks.Instance.Specials.Concrete.Place(world, level, position);
    }
}
