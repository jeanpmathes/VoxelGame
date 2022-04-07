// <copyright file="GateBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A simple gate that can be used in fences and walls. It can be opened and closed.
///     Data bit usage: <c>---coo</c>
/// </summary>
public class GateBlock : Block, IWideConnectable, IFlammable, IFillable
{
    private readonly List<BlockMesh> meshes = new(capacity: 8);

    private readonly List<BoundingVolume> volumes = new();

    internal GateBlock(string name, string namedId, string closedModel, string openModel) :
        base(
            name,
            namedId,
            BlockFlags.Functional,
            BoundingVolume.Block,
            TargetBuffer.Complex)
    {
        BlockModel closed = BlockModel.Load(closedModel);
        BlockModel open = BlockModel.Load(openModel);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) closedModels =
            closed.CreateAllOrientations(rotateTopAndBottomTexture: false);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) openModels =
            open.CreateAllOrientations(rotateTopAndBottomTexture: false);

        for (uint data = 0b00_0000; data <= 0b00_0111; data++)
        {
            var orientation = (Orientation) (data & 0b00_0011);
            bool isClosed = (data & 0b00_0100) == 0;

            BlockMesh mesh = orientation.Pick(isClosed ? closedModels : openModels).Mesh;
            meshes.Add(mesh);

            volumes.Add(CreateVolume(data));
        }
    }

    /// <inheritdoc />
    public bool IsConnectable(World world, BlockSide side, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block || block.Block != this) return false;

        var orientation = (Orientation) (block.Data & 0b00_0011);

        return orientation switch
        {
            Orientation.North => side is BlockSide.Left or BlockSide.Right,
            Orientation.East => side is BlockSide.Front or BlockSide.Back,
            Orientation.South => side is BlockSide.Left or BlockSide.Right,
            Orientation.West => side is BlockSide.Front or BlockSide.Back,
            _ => false
        };
    }

    private static BoundingVolume CreateVolume(uint data)
    {
        bool isClosed = (data & 0b00_0100) == 0;

        return (Orientation) (data & 0b00_0011) switch
        {
            Orientation.North => NorthSouth(offset: 0.375f),
            Orientation.East => WestEast(offset: 0.625f),
            Orientation.South => NorthSouth(offset: 0.625f),
            Orientation.West => WestEast(offset: 0.375f),
            _ => NorthSouth(offset: 0.375f)
        };

        BoundingVolume NorthSouth(float offset)
        {
            if (isClosed)
                return new BoundingVolume(
                    new Vector3(x: 0.96875f, y: 0.71875f, z: 0.5f),
                    new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f),
                    new BoundingVolume(
                        new Vector3(x: 0.96875f, y: 0.28125f, z: 0.5f),
                        new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    new BoundingVolume(
                        new Vector3(x: 0.03125f, y: 0.71875f, z: 0.5f),
                        new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    new BoundingVolume(
                        new Vector3(x: 0.03125f, y: 0.28125f, z: 0.5f),
                        new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    // Moving parts.
                    new BoundingVolume(
                        new Vector3(x: 0.75f, y: 0.71875f, z: 0.5f),
                        new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3(x: 0.75f, y: 0.28125f, z: 0.5f),
                        new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3(x: 0.25f, y: 0.71875f, z: 0.5f),
                        new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3(x: 0.25f, y: 0.28125f, z: 0.5f),
                        new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)));

            return new BoundingVolume(
                new Vector3(x: 0.96875f, y: 0.71875f, z: 0.5f),
                new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f),
                new BoundingVolume(
                    new Vector3(x: 0.96875f, y: 0.28125f, z: 0.5f),
                    new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                new BoundingVolume(
                    new Vector3(x: 0.03125f, y: 0.71875f, z: 0.5f),
                    new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                new BoundingVolume(
                    new Vector3(x: 0.03125f, y: 0.28125f, z: 0.5f),
                    new Vector3(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                // Moving parts.
                new BoundingVolume(
                    new Vector3(x: 0.875f, y: 0.71875f, offset),
                    new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3(x: 0.875f, y: 0.28125f, offset),
                    new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3(x: 0.125f, y: 0.71875f, offset),
                    new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3(x: 0.125f, y: 0.28125f, offset),
                    new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)));
        }

        BoundingVolume WestEast(float offset)
        {
            if (isClosed)
                return new BoundingVolume(
                    new Vector3(x: 0.5f, y: 0.71875f, z: 0.96875f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f),
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.28125f, z: 0.96875f),
                        new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.71875f, z: 0.03125f),
                        new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.28125f, z: 0.03125f),
                        new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    // Moving parts.
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.71875f, z: 0.75f),
                        new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.28125f, z: 0.75f),
                        new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.71875f, z: 0.25f),
                        new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3(x: 0.5f, y: 0.28125f, z: 0.25f),
                        new Vector3(x: 0.0625f, y: 0.09375f, z: 0.1875f)));

            return new BoundingVolume(
                new Vector3(x: 0.5f, y: 0.71875f, z: 0.96875f),
                new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f),
                new BoundingVolume(
                    new Vector3(x: 0.5f, y: 0.28125f, z: 0.96875f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                new BoundingVolume(
                    new Vector3(x: 0.5f, y: 0.71875f, z: 0.03125f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                new BoundingVolume(
                    new Vector3(x: 0.5f, y: 0.28125f, z: 0.03125f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                // Moving parts.
                new BoundingVolume(
                    new Vector3(offset, y: 0.71875f, z: 0.875f),
                    new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3(offset, y: 0.28125f, z: 0.875f),
                    new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3(offset, y: 0.71875f, z: 0.125f),
                    new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3(offset, y: 0.28125f, z: 0.125f),
                    new Vector3(x: 0.1875f, y: 0.09375f, z: 0.0625f)));
        }
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b00_0111];
    }

    /// <inheritdoc />
    public override BlockMeshData GetMesh(BlockMeshInfo info)
    {
        return meshes[(int) info.Data & 0b00_0111].GetComplexMeshData();
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        bool connectX = CheckOrientation(world, position, Orientation.East) ||
                        CheckOrientation(world, position, Orientation.West);

        bool connectZ = CheckOrientation(world, position, Orientation.South) ||
                        CheckOrientation(world, position, Orientation.North);

        return connectX || connectZ;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;

        bool connectX = CheckOrientation(world, position, Orientation.East) ||
                        CheckOrientation(world, position, Orientation.West);

        bool connectZ = CheckOrientation(world, position, Orientation.South) ||
                        CheckOrientation(world, position, Orientation.North);

        if (orientation.IsZ() && !connectX) orientation = orientation.Rotate();
        else if (orientation.IsX() && !connectZ) orientation = orientation.Rotate();

        world.SetBlock(this.AsInstance((uint) orientation), position);
    }

    private static bool CheckOrientation(World world, Vector3i position, Orientation orientation)
    {
        Vector3i otherPosition = orientation.Offset(position);

        return world.GetBlock(otherPosition)?.Block is IWideConnectable connectable &&
               connectable.IsConnectable(world, orientation.ToBlockSide().Opposite(), otherPosition);
    }

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        var orientation = (Orientation) (data & 0b00_0011);
        bool isClosed = (data & 0b00_0100) == 0;

        // Check if orientation has to be inverted.
        if (isClosed &&
            Vector2.Dot(
                orientation.ToVector3().Xz,
                entity.Position.Xz - new Vector2(position.X + 0.5f, position.Z + 0.5f)) < 0)
            orientation = orientation.Opposite();

        Vector3 center = isClosed
            ? new Vector3(x: 0.5f, y: 0.5f, z: 0.5f) + -orientation.ToVector3() * 0.09375f
            : new Vector3(x: 0.5f, y: 0.5f, z: 0.5f);

        float closedOffset = isClosed ? 0.09375f : 0f;

        Vector3 extents = orientation is Orientation.North or Orientation.South
            ? new Vector3(x: 0.5f, y: 0.375f, 0.125f + closedOffset)
            : new Vector3(0.125f + closedOffset, y: 0.375f, z: 0.5f);

        BoundingVolume volume = new(center, extents);

        if (entity.Collider.Intersects(volume.GetColliderAt(position))) return;

        entity.World.SetBlock(
            this.AsInstance((uint) ((isClosed ? 0b00_0100 : 0b00_0000) | (int) orientation.Opposite())),
            position);
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        var blockOrientation = (Orientation) (data & 0b00_0011);

        if (blockOrientation.Axis() != side.Axis().Rotate()) return;

        bool valid =
            CheckOrientation(world, position, side.ToOrientation()) ||
            CheckOrientation(world, position, side.ToOrientation().Opposite());

        if (!valid) Destroy(world, position);
    }
}
