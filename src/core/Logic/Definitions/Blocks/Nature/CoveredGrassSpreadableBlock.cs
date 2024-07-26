// <copyright file="BurnedGrassBlock.cs" company="VoxelGame">
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
///     A <see cref="CoveredDirtBlock" /> on that grass can spread. It models a dirt block with something on it that can be
///     washed away.
/// </summary>
public class CoveredGrassSpreadableBlock : CoveredDirtBlock, IGrassSpreadable
{
    internal CoveredGrassSpreadableBlock(String name, String namedID, TextureLayout normal, Boolean hasNeutralTint) :
        base(
            name,
            namedID,
            normal,
            normal,
            hasNeutralTint,
            supportsFullGrowth: false) {}

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid) world.SetBlock(Elements.Blocks.Instance.Dirt.AsInstance(), position);
    }
}
