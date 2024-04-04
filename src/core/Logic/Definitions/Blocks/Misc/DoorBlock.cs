// <copyright file="DoorBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A two units high block that can be opened and closed.
///     Data bit usage: <c>-csboo</c>
/// </summary>
// c: closed
// s: side
// b: base
// o: orientation
public class DoorBlock : Block, IFillable, IComplex
{
    private readonly List<BlockMesh> baseClosedMeshes = [];
    private readonly List<BlockMesh> baseOpenMeshes = [];

    private readonly BoundingVolume doorVolume = new(
        new Vector3d(x: 0.5f, y: 1f, z: 0.5f),
        new Vector3d(x: 0.5f, y: 1f, z: 0.5f));

    private readonly List<BlockMesh> topClosedMeshes = [];
    private readonly List<BlockMesh> topOpenMeshes = [];

    private readonly List<BoundingVolume> volumes = [];

    internal DoorBlock(String name, String namedID, String closedModel, String openModel) :
        base(
            name,
            namedID,
            BlockFlags.Functional with {IsOpaque = true},
            new BoundingVolume(new Vector3d(x: 0.5f, y: 1f, z: 0.5f), new Vector3d(x: 0.5f, y: 1f, z: 0.5f)))
    {
        BlockModel.Load(closedModel).PlaneSplit(
            Vector3d.UnitY,
            -Vector3d.UnitY,
            out BlockModel baseClosed,
            out BlockModel topClosed);

        topClosed.Move(-Vector3d.UnitY);

        BlockModel.Load(openModel).PlaneSplit(
            Vector3d.UnitY,
            -Vector3d.UnitY,
            out BlockModel baseOpen,
            out BlockModel topOpen);

        topOpen.Move(-Vector3d.UnitY);

        CreateMeshes(baseClosed, baseClosedMeshes);
        CreateMeshes(baseOpen, baseOpenMeshes);

        CreateMeshes(topClosed, topClosedMeshes);
        CreateMeshes(topOpen, topOpenMeshes);

        static void CreateMeshes(BlockModel model, ICollection<BlockMesh> meshList)
        {
            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) =
                model.CreateAllOrientations(rotateTopAndBottomTexture: true);

            meshList.Add(north.Mesh);
            meshList.Add(east.Mesh);
            meshList.Add(south.Mesh);
            meshList.Add(west.Mesh);
        }

        for (UInt32 data = 0; data <= 0b01_1111; data++) volumes.Add(CreateVolume(data));
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        var orientation = (Orientation) (info.Data & 0b00_0011);
        Boolean isBase = (info.Data & 0b00_0100) == 0;
        Boolean isLeftSided = (info.Data & 0b00_1000) == 0;
        Boolean isClosed = (info.Data & 0b01_0000) == 0;

        if (isClosed)
        {
            var index = (Int32) orientation;

            BlockMesh mesh = isBase ? baseClosedMeshes[index] : topClosedMeshes[index];

            return mesh.GetMeshData();
        }
        else
        {
            Orientation openOrientation = isLeftSided ? orientation.Opposite() : orientation;
            var index = (Int32) openOrientation;

            BlockMesh mesh = isBase ? baseOpenMeshes[index] : topOpenMeshes[index];

            return mesh.GetMeshData();
        }
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        var orientation = (Orientation) (data & 0b00_0011);

        // Check if door is open and if the door is left sided.
        if ((data & 0b01_0000) != 0)
            orientation = (data & 0b00_1000) == 0 ? orientation.Rotate() : orientation.Rotate().Opposite();

        return orientation switch
        {
            Orientation.North => new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.9375f),
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.0625f)),
            Orientation.East => new BoundingVolume(
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.5f)),
            Orientation.South => new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.0625f),
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.0625f)),
            Orientation.West => new BoundingVolume(
                new Vector3d(x: 0.9375f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.5f)),
            _ => new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f))
        };
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b01_1111];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return world.GetBlock(position.Above())?.Block.IsReplaceable == true &&
               world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Orientation orientation = actor?.LookingDirection.ToOrientation() ?? Orientation.North;
        BlockSide side = actor?.TargetSide ?? BlockSide.Top;

        Boolean isLeftSided = ChooseIfLeftSided(world, position, side, orientation);

        world.SetBlock(this.AsInstance((UInt32) ((isLeftSided ? 0b0000 : 0b1000) | (Int32) orientation)), position);

        world.SetBlock(
            this.AsInstance((UInt32) ((isLeftSided ? 0b0000 : 0b1000) | 0b0100 | (Int32) orientation)),
            position.Above());
    }

    private Boolean ChooseIfLeftSided(World world, Vector3i position, BlockSide side, Orientation orientation)
    {
        Boolean isLeftSided;

        if (side == BlockSide.Top)
        {
            // Choose side according to neighboring doors to form a double door.

            Orientation toNeighbor = orientation.Rotate().Opposite();
            Vector3i neighborPosition = toNeighbor.Offset(position);

            (Block block, UInt32 data) = world.GetBlock(neighborPosition) ?? BlockInstance.Default;
            isLeftSided = block != this || (data & 0b00_1011) != (Int32) orientation;
        }
        else
        {
            isLeftSided = orientation.Rotate().Opposite().ToBlockSide() != side;
        }

        return isLeftSided;
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, UInt32 data, PhysicsActor? actor)
    {
        Boolean isBase = (data & 0b00_0100) == 0;

        world.SetDefaultBlock(position);
        world.SetDefaultBlock(position + (isBase ? Vector3i.UnitY : -Vector3i.UnitY));
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        Boolean isBase = (data & 0b00_0100) == 0;
        Vector3i otherPosition = position + (isBase ? Vector3i.UnitY : -Vector3i.UnitY);

        if (actor.Collider.Intersects(doorVolume.GetColliderAt(otherPosition))) return;

        actor.World.SetBlock(this.AsInstance(data ^ 0b1_0000), position);
        actor.World.SetBlock(this.AsInstance(data ^ 0b1_0100), otherPosition);

        // Open a neighboring door, if available.
        Boolean isLeftSided = (data & 0b00_1000) == 0;
        var orientation = (Orientation) (data & 0b00_0011);
        orientation = isLeftSided ? orientation.Opposite() : orientation;

        Orientation toNeighbor = orientation.Rotate().Opposite();

        OpenNeighbor(toNeighbor.Offset(position));

        void OpenNeighbor(Vector3i neighborPosition)
        {
            (Block block, UInt32 u) = actor.World.GetBlock(neighborPosition) ?? BlockInstance.Default;

            if (block == this && (data & 0b01_1011) == ((u ^ 0b00_1000) & 0b01_1011))
                block.ActorInteract(actor, neighborPosition);
        }
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, BlockSide side)
    {
        if (side == BlockSide.Bottom && (data & 0b00_0100) == 0 && !world.HasFullAndSolidGround(position))
            Destroy(world, position);
    }
}
