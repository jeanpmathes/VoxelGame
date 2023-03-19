﻿// <copyright file="BurnedGrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A <see cref="CoveredDirtBlock" /> on that grass can spread. It models a dirt block with something on it that can be
///     washed away.
/// </summary>
public class CoveredGrassSpreadableBlock : CoveredDirtBlock, IGrassSpreadable, IFillable
{
    internal CoveredGrassSpreadableBlock(string name, string namedId, TextureLayout normal, bool hasNeutralTint) :
        base(
            name,
            namedId,
            normal,
            normal,
            hasNeutralTint,
            supportsFullGrowth: false) {}

    /// <inheritdoc />
    public void OnFluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid.IsFluid) world.SetBlock(Logic.Blocks.Instance.Dirt.AsInstance(), position);
    }
}


