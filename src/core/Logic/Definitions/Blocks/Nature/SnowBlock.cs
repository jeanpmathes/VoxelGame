// <copyright file="SnowBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
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
    internal SnowBlock(string name, string namedID, TextureLayout layout) : base(name, namedID, layout) {}

    /// <inheritdoc />
    public bool IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
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
