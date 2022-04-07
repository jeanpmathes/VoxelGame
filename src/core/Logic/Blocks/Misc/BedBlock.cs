﻿// <copyright file="BedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A block that is two blocks long and allows setting the spawn point.
///     Data bit usage: <c>cccoop</c>
/// </summary>
// c: color
// o: orientation
// p: position
public class BedBlock : Block, IFlammable, IFillable
{
    private readonly List<BlockMesh> footMeshes = new(capacity: 4);
    private readonly List<BlockMesh> headMeshes = new(capacity: 4);

    private readonly List<BoundingVolume> volumes = new();

    internal BedBlock(string name, string namedId, string model) :
        base(
            name,
            namedId,
            BlockFlags.Functional,
            new BoundingVolume(
                new Vector3(x: 0.5f, y: 0.21875f, z: 0.5f),
                new Vector3(x: 0.5f, y: 0.21875f, z: 0.5f)),
            TargetBuffer.Complex)
    {
        BlockModel blockModel = BlockModel.Load(model);

        blockModel.PlaneSplit(Vector3.UnitZ, Vector3.UnitZ, out BlockModel foot, out BlockModel head);
        foot.Move(-Vector3.UnitZ);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) headParts =
            head.CreateAllOrientations(rotateTopAndBottomTexture: true);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) footParts =
            foot.CreateAllOrientations(rotateTopAndBottomTexture: true);

        headParts.Lock();
        footParts.Lock();

        headMeshes.Add(headParts.north.Mesh);
        footMeshes.Add(footParts.north.Mesh);

        headMeshes.Add(headParts.east.Mesh);
        footMeshes.Add(footParts.east.Mesh);

        headMeshes.Add(headParts.south.Mesh);
        footMeshes.Add(footParts.south.Mesh);

        headMeshes.Add(headParts.west.Mesh);
        footMeshes.Add(footParts.west.Mesh);

        for (uint data = 0; data <= 0b11_1111; data++) volumes.Add(CreateVolume(data));
    }

    private static BoundingVolume CreateVolume(uint data)
    {
        bool isBase = (data & 0b1) == 1;
        var orientation = (Orientation) ((data & 0b00_0110) >> 1);

        var legs = new BoundingVolume[2];

        switch (isBase ? orientation : orientation.Opposite())
        {
            case Orientation.North:

                legs[0] = new BoundingVolume(
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                legs[1] = new BoundingVolume(
                    new Vector3(x: 0.90625f, y: 0.09375f, z: 0.09375f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                break;

            case Orientation.East:

                legs[0] = new BoundingVolume(
                    new Vector3(x: 0.90625f, y: 0.09375f, z: 0.09375f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                legs[1] = new BoundingVolume(
                    new Vector3(x: 0.90625f, y: 0.09375f, z: 0.90625f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                break;

            case Orientation.South:

                legs[0] = new BoundingVolume(
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.90625f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                legs[1] = new BoundingVolume(
                    new Vector3(x: 0.90625f, y: 0.09375f, z: 0.90625f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                break;

            case Orientation.West:

                legs[0] = new BoundingVolume(
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                legs[1] = new BoundingVolume(
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.90625f),
                    new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                break;

            default: throw new InvalidOperationException();
        }

        return new BoundingVolume(
            new Vector3(x: 0.5f, y: 0.3125f, z: 0.5f),
            new Vector3(x: 0.5f, y: 0.125f, z: 0.5f),
            legs);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b11_1111];
    }

    /// <inheritdoc />
    public override BlockMeshData GetMesh(BlockMeshInfo info)
    {
        bool isHead = (info.Data & 0b1) == 1;
        var orientation = (int) ((info.Data & 0b00_0110) >> 1);
        var color = (BlockColor) ((info.Data & 0b11_1000) >> 3);

        BlockMesh mesh = isHead ? headMeshes[orientation] : footMeshes[orientation];

        return mesh.GetComplexMeshData(color.ToTintColor());
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        if (!world.HasSolidGround(position, solidify: true)) return false;

        Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
        Vector3i otherPosition = orientation.Offset(position);

        return world.GetBlock(otherPosition)?.Block.IsReplaceable == true &&
               world.HasSolidGround(otherPosition, solidify: true);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
        Vector3i otherPosition = orientation.Offset(position);

        world.SetBlock(this.AsInstance((uint) orientation << 1), position);
        world.SetBlock(this.AsInstance((uint) (((int) orientation << 1) | 1)), otherPosition);

        world.SpawnPosition = new Vector3(position.X, y: 1024f, position.Z);
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
    {
        bool isHead = (data & 0b1) == 1;
        var orientation = (Orientation) ((data & 0b00_0110) >> 1);
        Orientation placementOrientation = isHead ? orientation.Opposite() : orientation;

        world.SetDefaultBlock(position);
        world.SetDefaultBlock(placementOrientation.Offset(position));
    }

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        bool isHead = (data & 0b1) == 1;

        var orientation = (Orientation) ((data & 0b00_0110) >> 1);
        Orientation placementOrientation = isHead ? orientation.Opposite() : orientation;

        entity.World.SetBlock(this.AsInstance((data + 0b00_1000) & 0b11_1111), position);

        entity.World.SetBlock(
            this.AsInstance(((data + 0b00_1000) & 0b11_1111) ^ 0b00_0001),
            placementOrientation.Offset(position));
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side == BlockSide.Bottom && !world.HasSolidGround(position)) Destroy(world, position);
    }
}
