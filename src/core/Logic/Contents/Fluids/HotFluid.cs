// <copyright file="HotFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Contents.Fluids;

/// <summary>
///     A fluid that can burn its surroundings.
/// </summary>
public class HotFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="HotFluid" />.
    /// </summary>
    public HotFluid(String name, String namedID, Single density, Int32 viscosity, Boolean hasNeutralTint,
        TID texture, RenderType renderType = RenderType.Opaque) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint,
            texture,
            renderType) {}

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidInstance instance)
    {
        if (world.GetBlock(position)?.Block.Get<Combustible>() is {} combustible) combustible.DoBurn(world, position, Blocks.Instance.Environment.Fire);

        BurnAround(world, position);

        base.ScheduledUpdate(world, position, instance);
    }

    /// <inheritdoc />
    internal override void DoRandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic)
    {
        BurnAround(world, position);
    }

    private static void BurnAround(World world, Vector3i position)
    {
        foreach (Side side in Side.All.Sides())
        {
            Vector3i offsetPosition = side.Offset(position);

            if (world.GetBlock(offsetPosition)?.Block.Get<Combustible>() is {} combustible &&
                combustible.DoBurn(world, offsetPosition, Blocks.Instance.Environment.Fire))
                Blocks.Instance.Environment.Fire.Place(world, offsetPosition);

        }
    }
}
