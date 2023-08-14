// <copyright file="SaltBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block of salt.
/// </summary>
public class SaltBlock : GroundedModifiableHeightBlock, IFillable
{
    internal SaltBlock(string name, string namedID, TextureLayout layout) : base(name, namedID, layout) {}

    /// <inheritdoc />
    public bool IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.IsFluid;
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.IsEmpty) return;

        Destroy(world, position);

        if (content.Fluid is {Fluid: var fluid, Level: FluidLevel.One}
            && fluid == Logic.Fluids.Instance.FreshWater)
            world.SetFluid(Logic.Fluids.Instance.SeaWater.AsInstance(FluidLevel.One), position);
    }
}
