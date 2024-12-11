// <copyright file="SnowBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A snow block.
/// </summary>
public class SnowBlock : GroundedModifiableHeightBlock, IFillable
{
    /// <summary>
    ///     Creates a new instance of the <see cref="SnowBlock" /> class.
    /// </summary>
    internal SnowBlock(String name, String namedID, TextureLayout layout) : base(name, namedID, layout) {}

    /// <inheritdoc />
    public Boolean IsInflowAllowed(World world, Vector3i position, Side side, Fluid fluid)
    {
        BlockInstance? block = world.GetBlock(position);

        if (block == null) return false;

        return GetHeight(block.Value.Data) < IVaryingHeight.HalfHeight;
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (!content.Fluid.IsEmpty) ScheduleDestroy(world, position);
    }
}
