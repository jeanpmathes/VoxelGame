// <copyright file="SaltBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block of salt.
/// </summary>
public class SaltBlock : GroundedModifiableHeightBlock, IFillable
{
    internal SaltBlock(String name, String namedID, TextureLayout layout) : base(name, namedID, layout) {}

    /// <inheritdoc />
    public Boolean IsInflowAllowed(World world, Vector3i position, Side side, Fluid fluid)
    {
        return fluid.IsFluid;
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.IsEmpty) return;

        Destroy(world, position);

        if (content.Fluid is {Fluid: var fluid, Level: FluidLevel.One}
            && fluid == Elements.Fluids.Instance.FreshWater)
            world.SetFluid(Elements.Fluids.Instance.SeaWater.AsInstance(FluidLevel.One), position);
    }
}
