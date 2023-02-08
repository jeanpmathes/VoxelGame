// <copyright file="VaryingHeightBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics;
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

    private readonly List<BoundingVolume> volumes = new();
    private int[] textureIndices = null!;

    /// <inheritdoc />
    protected VaryingHeightBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedId,
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
    public virtual int GetHeight(uint data)
    {
        return (int) (data & 0b00_1111);
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        return new IVaryingHeight.MeshData
        {
            TextureIndex = textureIndices[(int) info.Side],
            Tint = TintColor.None
        };
    }

    /// <summary>
    ///     Get an instance of this block with the given height.
    /// </summary>
    /// <param name="height">The height of the block, in the range [0, 15].</param>
    /// <returns>The block instance.</returns>
    public BlockInstance GetInstance(int height)
    {
        Debug.Assert(height >= 0 && height <= IVaryingHeight.MaximumHeight);

        return this.AsInstance((uint) height);
    }

    private void CreateVolumes()
    {
        for (uint data = 0; data <= 0b00_1111; data++) volumes.Add(BoundingVolume.BlockWithHeight(GetHeight(data)));
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        textureIndices = layout.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b00_1111];
    }
}

