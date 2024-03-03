// <copyright file="HotFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Fluids;

/// <summary>
///     A fluid that can burn it's surroundings.
/// </summary>
public class HotFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="HotFluid" />.
    /// </summary>
    public HotFluid(string name, string namedID, float density, int viscosity, bool hasNeutralTint,
        TextureLayout movingLayout, TextureLayout staticLayout,
        RenderType renderType = RenderType.Opaque) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint,
            movingLayout,
            staticLayout,
            renderType) {}

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidInstance instance)
    {
        if (world.GetBlock(position)?.Block is ICombustible block) block.Burn(world, position, Logic.Blocks.Instance.Fire);

        BurnAround(world, position);

        base.ScheduledUpdate(world, position, instance);
    }

    /// <inheritdoc />
    internal override void RandomUpdate(World world, Vector3i position, FluidLevel level, bool isStatic)
    {
        BurnAround(world, position);
    }

    private static void BurnAround(World world, Vector3i position)
    {
        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Vector3i offsetPosition = side.Offset(position);

            if (world.GetBlock(offsetPosition)?.Block is ICombustible block &&
                block.Burn(world, offsetPosition, Logic.Blocks.Instance.Fire))
                Logic.Blocks.Instance.Fire.Place(world, offsetPosition);

        }
    }
}
