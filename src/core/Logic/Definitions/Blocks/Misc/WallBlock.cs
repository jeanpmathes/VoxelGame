// <copyright file="WallBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     This class represents a wall block which connects to blocks with the
///     <see cref="VoxelGame.Core.Logic.Interfaces.IWideConnectable" /> interface. When connecting in a straight line, no
///     post is used and indices are not ignored, else indices are ignored.
///     Data bit usage: <c>--nesw</c>
/// </summary>
// n: connected north
// e: connected east
// s: connected south
// w: connected west
public class WallBlock : WideConnectingBlock
{
    private readonly String texture;
    private readonly RID extensionStraight;

    private readonly List<BoundingVolume> volumes = [];

    private BlockMesh straightX = null!;
    private BlockMesh straightZ = null!;

    internal WallBlock(String name, String namedID, String texture,
        RID postModel, RID extensionModel, RID extensionStraight) :
        base(
            name,
            namedID,
            texture,
            isOpaque: true,
            postModel,
            extensionModel,
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.25f, y: 0.5f, z: 0.25f)))
    {
        this.texture = texture;
        this.extensionStraight = extensionStraight;
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        BlockModel straightZModel = modelProvider.GetModel(extensionStraight);
        straightZModel.OverwriteTexture(texture);

        BlockModel straightXModel = straightZModel.Copy();
        straightXModel.RotateY(rotations: 1, rotateTopAndBottomTexture: false);

        straightX = straightXModel.CreateMesh(textureIndexProvider);
        straightZ = straightZModel.CreateMesh(textureIndexProvider);

        for (UInt32 data = 0; data <= 0b00_1111; data++) volumes.Add(CreateVolume(data));
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        Boolean north = (data & 0b00_1000) != 0;
        Boolean east = (data & 0b00_0100) != 0;
        Boolean south = (data & 0b00_0010) != 0;
        Boolean west = (data & 0b00_0001) != 0;

        Boolean useStraightZ = north && south && !east && !west;
        Boolean useStraightX = !north && !south && east && west;

        if (useStraightZ)
            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.5f));

        if (useStraightX)
            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.1875f));

        Int32 extensions = BitHelper.CountSetBits(data & 0b1111);

        var children = new BoundingVolume[extensions];
        extensions = 0;

        if (north)
        {
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.125f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.125f));

            extensions++;
        }

        if (east)
        {
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.875f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.1875f));

            extensions++;
        }

        if (south)
        {
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.875f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.125f));

            extensions++;
        }

        if (west)
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.1875f));

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.25f, y: 0.5f, z: 0.25f),
            children);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_1111];
    }

    /// <inheritdoc />
    protected override IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        Boolean north = (info.Data & 0b00_1000) != 0;
        Boolean east = (info.Data & 0b00_0100) != 0;
        Boolean south = (info.Data & 0b00_0010) != 0;
        Boolean west = (info.Data & 0b00_0001) != 0;

        Boolean useStraightZ = north && south && !east && !west;
        Boolean useStraightX = !north && !south && east && west;

        if (useStraightZ || useStraightX)
            return useStraightZ ? straightZ.GetMeshData() : straightX.GetMeshData();

        return base.GetMeshData(info);
    }
}
