// <copyright file="WallBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Blocks;

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
    private readonly BlockMesh straightX;
    private readonly BlockMesh straightZ;

    private readonly List<BoundingVolume> volumes = new();

    internal WallBlock(string name, string namedId, string texture, string postModel, string extensionModel,
        string extensionStraight) :
        base(
            name,
            namedId,
            texture,
            postModel,
            extensionModel,
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.25f, y: 0.5f, z: 0.25f)))
    {
        BlockModel straightZModel = BlockModel.Load(extensionStraight);
        straightZModel.OverwriteTexture(texture);

        BlockModel straightXModel = straightZModel.Copy();
        straightXModel.RotateY(rotations: 1, rotateTopAndBottomTexture: false);

        straightX = straightXModel.Mesh;
        straightZ = straightZModel.Mesh;

        for (uint data = 0; data <= 0b00_1111; data++) volumes.Add(CreateVolume(data));
    }

    private static BoundingVolume CreateVolume(uint data)
    {
        bool north = (data & 0b00_1000) != 0;
        bool east = (data & 0b00_0100) != 0;
        bool south = (data & 0b00_0010) != 0;
        bool west = (data & 0b00_0001) != 0;

        bool useStraightZ = north && south && !east && !west;
        bool useStraightX = !north && !south && east && west;

        if (useStraightZ)
            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.5f));

        if (useStraightX)
            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.1875f));

        int extensions = BitHelper.CountSetBits(data & 0b1111);

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
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b00_1111];
    }

    /// <inheritdoc />
    protected override IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        bool north = (info.Data & 0b00_1000) != 0;
        bool east = (info.Data & 0b00_0100) != 0;
        bool south = (info.Data & 0b00_0010) != 0;
        bool west = (info.Data & 0b00_0001) != 0;

        bool useStraightZ = north && south && !east && !west;
        bool useStraightX = !north && !south && east && west;

        if (useStraightZ || useStraightX)
            return useStraightZ ? straightZ.GetMeshData() : straightX.GetMeshData();

        return base.GetMeshData(info);
    }
}
