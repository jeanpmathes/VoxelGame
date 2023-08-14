// <copyright file="SaltWaterFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
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
    public SaltWaterFluid(string name, string namedID, float density, int viscosity, TextureLayout movingLayout, TextureLayout staticLayout) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint: true,
            movingLayout,
            staticLayout,
            RenderType.Transparent) {}

    /// <inheritdoc />
    internal override void RandomUpdate(World world, Vector3i position, FluidLevel level, bool isStatic)
    {
        if (!isStatic || level != FluidLevel.One) return;
        if (!world.HasFullAndSolidGround(position)) return;

        world.SetDefaultFluid(position);
        Logic.Blocks.Instance.Specials.Salt.Place(world, level, position);
    }
}
