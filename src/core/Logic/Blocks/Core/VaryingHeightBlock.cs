﻿// <copyright file="VaryingHeightBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A block that can have different heights.
///     Data bit usage: <c>--hhhh</c>
/// </summary>
// h: height
public class VaryingHeightBlock : Block, IHeightVariable
{
    private readonly TextureLayout layout;

    private readonly List<BoundingVolume> volumes = new();
    private int[] textureIndices = null!;

    /// <inheritdoc />
    protected VaryingHeightBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedId,
            flags with { IsFull = false, IsOpaque = false },
            BoundingVolume.Block,
            TargetBuffer.VaryingHeight)
    {
        this.layout = layout;

        CreateVolumes();
    }

    /// <inheritdoc />
    public virtual int GetHeight(uint data)
    {
        return (int) (data & 0b00_1111);
    }

    private void CreateVolumes()
    {
        for (uint data = 0; data <= 0b00_1111; data++) volumes.Add(BoundingVolume.BlockWithHeight(GetHeight(data)));
    }

    /// <inheritdoc />
    protected override void Setup(ITextureIndexProvider indexProvider)
    {
        textureIndices = layout.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b00_1111];
    }

    /// <inheritdoc />
    public override BlockMeshData GetMesh(BlockMeshInfo info)
    {
        return BlockMeshData.VaryingHeight(textureIndices[(int) info.Side], TintColor.None);
    }
}
