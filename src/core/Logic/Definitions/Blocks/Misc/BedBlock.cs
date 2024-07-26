﻿// <copyright file="BedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that is two blocks long and allows setting the spawn point.
///     Data bit usage: <c>cccoop</c>
/// </summary>
// c: color
// o: orientation
// p: position
public class BedBlock : Block, ICombustible, IFillable, IComplex
{
    private readonly List<BlockMesh> footMeshes = new(capacity: 4);
    private readonly List<BlockMesh> headMeshes = new(capacity: 4);

    private readonly List<BoundingVolume> volumes = [];

    internal BedBlock(String name, String namedID, String model) :
        base(
            name,
            namedID,
            BlockFlags.Functional with {IsOpaque = true},
            new BoundingVolume(
                new Vector3d(x: 0.5, y: 0.21875, z: 0.5),
                new Vector3d(x: 0.5, y: 0.21875, z: 0.5)))
    {
        BlockModel blockModel = BlockModel.Load(model);

        blockModel.PlaneSplit(Vector3d.UnitZ, Vector3d.UnitZ, out BlockModel foot, out BlockModel head);
        foot.Move(-Vector3d.UnitZ);

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

        for (UInt32 data = 0; data <= 0b11_1111; data++) volumes.Add(CreateVolume(data));
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        Boolean isHead = (info.Data & 0b1) == 1;
        var orientation = (Int32) ((info.Data & 0b00_0110) >> 1);
        var color = (BlockColor) ((info.Data & 0b11_1000) >> 3);

        BlockMesh mesh = isHead ? headMeshes[orientation] : footMeshes[orientation];

        return mesh.GetMeshData(color.ToTintColor());
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        Boolean isBase = (data & 0b1) == 1;
        var orientation = (Orientation) ((data & 0b00_0110) >> 1);

        var legs = new BoundingVolume[2];

        switch (isBase ? orientation : orientation.Opposite())
        {
            case Orientation.North:

                legs[0] = new BoundingVolume(
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                legs[1] = new BoundingVolume(
                    new Vector3d(x: 0.90625, y: 0.09375, z: 0.09375),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                break;

            case Orientation.East:

                legs[0] = new BoundingVolume(
                    new Vector3d(x: 0.90625, y: 0.09375, z: 0.09375),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                legs[1] = new BoundingVolume(
                    new Vector3d(x: 0.90625, y: 0.09375, z: 0.90625),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                break;

            case Orientation.South:

                legs[0] = new BoundingVolume(
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.90625),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                legs[1] = new BoundingVolume(
                    new Vector3d(x: 0.90625, y: 0.09375, z: 0.90625),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                break;

            case Orientation.West:

                legs[0] = new BoundingVolume(
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                legs[1] = new BoundingVolume(
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.90625),
                    new Vector3d(x: 0.09375, y: 0.09375, z: 0.09375));

                break;

            default: throw new InvalidOperationException();
        }

        return new BoundingVolume(
            new Vector3d(x: 0.5, y: 0.3125, z: 0.5),
            new Vector3d(x: 0.5, y: 0.125, z: 0.5),
            legs);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b11_1111];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        if (!world.HasFullAndSolidGround(position, solidify: true)) return false;

        Orientation orientation = actor?.Head.Forward.ToOrientation() ?? Orientation.North;
        Vector3i otherPosition = orientation.Offset(position);

        return world.GetBlock(otherPosition)?.Block.IsReplaceable == true &&
               world.HasFullAndSolidGround(otherPosition, solidify: true);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Orientation orientation = actor?.Head.Forward.ToOrientation() ?? Orientation.North;
        Vector3i otherPosition = orientation.Offset(position);

        world.SetBlock(this.AsInstance((UInt32) orientation << 1), position);
        world.SetBlock(this.AsInstance((UInt32) (((Int32) orientation << 1) | 1)), otherPosition);

        world.SpawnPosition = new Vector3d(position.X, position.Y + 1f, position.Z);
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, UInt32 data, PhysicsActor? actor)
    {
        Boolean isHead = (data & 0b1) == 1;
        var orientation = (Orientation) ((data & 0b00_0110) >> 1);
        Orientation placementOrientation = isHead ? orientation.Opposite() : orientation;

        world.SetDefaultBlock(position);
        world.SetDefaultBlock(placementOrientation.Offset(position));
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        Boolean isHead = (data & 0b1) == 1;

        var orientation = (Orientation) ((data & 0b00_0110) >> 1);
        Orientation placementOrientation = isHead ? orientation.Opposite() : orientation;

        actor.World.SetBlock(this.AsInstance((data + 0b00_1000) & 0b11_1111), position);

        actor.World.SetBlock(
            this.AsInstance(((data + 0b00_1000) & 0b11_1111) ^ 0b00_0001),
            placementOrientation.Offset(position));
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, BlockSide side)
    {
        if (side == BlockSide.Bottom && !world.HasFullAndSolidGround(position)) Destroy(world, position);
    }
}
