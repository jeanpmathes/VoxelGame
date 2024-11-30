// <copyright file="VaryingHeightBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that can have different heights.
///     Data bit usage: <c>--hhhh</c>
/// </summary>
// h: height
public class VaryingHeightBlock : Block, IVaryingHeight
{
    private readonly TextureLayout layout;

    private readonly List<BoundingVolume> volumes = [];
    private SideArray<Int32> textureIndices = null!;

    /// <inheritdoc />
    protected VaryingHeightBlock(String name, String namedID, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedID,
            flags with {IsFull = false},
            BoundingVolume.Block)
    {
        this.layout = layout;

        CreateVolumes();
    }

    /// <summary>
    ///     Get the block data for a full height block.
    /// </summary>
    public BlockInstance FullHeightInstance => GetInstance(IVaryingHeight.MaximumHeight);

    /// <inheritdoc />
    public virtual Int32 GetHeight(UInt32 data)
    {
        return (Int32) (data & 0b00_1111);
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        return new IVaryingHeight.MeshData
        {
            TextureIndex = textureIndices[info.Side],
            Tint = TintColor.None
        };
    }

    /// <summary>
    ///     Get an instance of this block with the given height.
    /// </summary>
    /// <param name="height">The height of the block, in the range [0, 15].</param>
    /// <returns>The block instance.</returns>
    public BlockInstance GetInstance(Int32 height)
    {
        Debug.Assert(height >= 0 && height <= IVaryingHeight.MaximumHeight);

        return this.AsInstance((UInt32) height);
    }

    /// <summary>
    ///     Place a block of this type at the given position, with the height according to the fluid level.
    /// </summary>
    /// <param name="world">The world to place the block in.</param>
    /// <param name="level">The fluid level.</param>
    /// <param name="position">The position to place the block at.</param>
    public void Place(World world, FluidLevel level, Vector3i position)
    {
        if (base.Place(world, position))
            world.SetBlock(GetInstance(level.GetBlockHeight()), position);
    }

    private void CreateVolumes()
    {
        for (UInt32 data = 0; data <= 0b00_1111; data++) volumes.Add(BoundingVolume.BlockWithHeight(GetHeight(data)));
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        textureIndices = layout.GetTextureIndices(indexProvider);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_1111];
    }
}
