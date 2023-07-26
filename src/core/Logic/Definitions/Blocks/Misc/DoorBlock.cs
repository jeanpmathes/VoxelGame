// <copyright file="DoorBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
    private readonly List<BlockMesh> baseClosedMeshes = new();
    private readonly List<BlockMesh> baseOpenMeshes = new();

    private readonly BoundingVolume doorVolume = new(
        new Vector3d(x: 0.5f, y: 1f, z: 0.5f),
        new Vector3d(x: 0.5f, y: 1f, z: 0.5f));

    private readonly List<BlockMesh> topClosedMeshes = new();
    private readonly List<BlockMesh> topOpenMeshes = new();

    private readonly List<BoundingVolume> volumes = new();

    internal DoorBlock(string name, string namedId, string closedModel, string openModel) :
        base(
            name,
            namedId,
            BlockFlags.Functional,
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

        for (uint data = 0; data <= 0b01_1111; data++) volumes.Add(CreateVolume(data));
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        var orientation = (Orientation) (info.Data & 0b00_0011);
        bool isBase = (info.Data & 0b00_0100) == 0;
        bool isLeftSided = (info.Data & 0b00_1000) == 0;
        bool isClosed = (info.Data & 0b01_0000) == 0;

        if (isClosed)
        {
            var index = (int) orientation;

            BlockMesh mesh = isBase ? baseClosedMeshes[index] : topClosedMeshes[index];

            return mesh.GetMeshData();
        }
        else
        {
            Orientation openOrientation = isLeftSided ? orientation.Opposite() : orientation;
            var index = (int) openOrientation;

            BlockMesh mesh = isBase ? baseOpenMeshes[index] : topOpenMeshes[index];

            return mesh.GetMeshData();
        }
    }

    private static BoundingVolume CreateVolume(uint data)
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
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b01_1111];
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.GetBlock(position.Above())?.Block.IsReplaceable == true &&
               world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
        BlockSide side = entity?.TargetSide ?? BlockSide.Top;

        bool isLeftSided = ChooseIfLeftSided(world, position, side, orientation);

        world.SetBlock(this.AsInstance((uint) ((isLeftSided ? 0b0000 : 0b1000) | (int) orientation)), position);

        world.SetBlock(
            this.AsInstance((uint) ((isLeftSided ? 0b0000 : 0b1000) | 0b0100 | (int) orientation)),
            position.Above());
    }

    private bool ChooseIfLeftSided(World world, Vector3i position, BlockSide side, Orientation orientation)
    {
        bool isLeftSided;

        if (side == BlockSide.Top)
        {
            // Choose side according to neighboring doors to form a double door.

            Orientation toNeighbor = orientation.Rotate().Opposite();
            Vector3i neighborPosition = toNeighbor.Offset(position);

            (Block block, uint data) = world.GetBlock(neighborPosition) ?? BlockInstance.Default;
            isLeftSided = block != this || (data & 0b00_1011) != (int) orientation;
        }
        else
        {
            isLeftSided = orientation.Rotate().Opposite().ToBlockSide() != side;
        }

        return isLeftSided;
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
    {
        bool isBase = (data & 0b00_0100) == 0;

        world.SetDefaultBlock(position);
        world.SetDefaultBlock(position + (isBase ? Vector3i.UnitY : -Vector3i.UnitY));
    }

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        bool isBase = (data & 0b00_0100) == 0;
        Vector3i otherPosition = position + (isBase ? Vector3i.UnitY : -Vector3i.UnitY);

        if (entity.Collider.Intersects(doorVolume.GetColliderAt(otherPosition))) return;

        entity.World.SetBlock(this.AsInstance(data ^ 0b1_0000), position);
        entity.World.SetBlock(this.AsInstance(data ^ 0b1_0100), otherPosition);

        // Open a neighboring door, if available.
        bool isLeftSided = (data & 0b00_1000) == 0;
        var orientation = (Orientation) (data & 0b00_0011);
        orientation = isLeftSided ? orientation.Opposite() : orientation;

        Orientation toNeighbor = orientation.Rotate().Opposite();

        OpenNeighbor(toNeighbor.Offset(position));

        void OpenNeighbor(Vector3i neighborPosition)
        {
            (Block block, uint u) = entity.World.GetBlock(neighborPosition) ?? BlockInstance.Default;

            if (block == this && (data & 0b01_1011) == ((u ^ 0b00_1000) & 0b01_1011))
                block.EntityInteract(entity, neighborPosition);
        }
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side == BlockSide.Bottom && (data & 0b00_0100) == 0 && !world.HasFullAndSolidGround(position))
            Destroy(world, position);
    }
}
