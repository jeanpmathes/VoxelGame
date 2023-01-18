// <copyright file="ConcreteFluid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
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
    /// <param name="namedId">The named ID of the fluid.</param>
    /// <param name="density">The density of the fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid.</param>
    /// <param name="movingLayout">The texture layout when this fluid is moving.</param>
    /// <param name="staticLayout">The texture layout when this fluid is static.</param>
    public ConcreteFluid(string name, string namedId, float density, int viscosity, TextureLayout movingLayout,
        TextureLayout staticLayout) :
        base(
            name,
            namedId,
            density,
            viscosity,
            neutralTint: false,
            movingLayout,
            staticLayout) {}

    /// <inheritdoc />
    internal override void RandomUpdate(World world, Vector3i position, FluidLevel level, bool isStatic)
    {
        if (!isStatic) return;

        world.SetDefaultFluid(position);
        Logic.Blocks.Instance.Specials.Concrete.Place(world, level, position);
    }
}

