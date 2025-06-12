// <copyright file="GateBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A simple gate that can be used in fences and walls. It can be opened and closed.
///     Data bit usage: <c>---coo</c>
/// </summary>
public class GateBlock : Block, IWideConnectable, ICombustible, IFillable, IComplex
{
    private readonly TID texture;
    private readonly RID closedModel;
    private readonly RID openModel;

    private readonly List<BlockMesh> meshes = new(capacity: 8);

    private readonly List<BoundingVolume> volumes = [];

    internal GateBlock(String name, String namedID, TID texture, RID closedModel, RID openModel) :
        base(
            name,
            namedID,
            BlockFlags.Functional,
            BoundingVolume.Block)
    {
        this.texture = texture;
        this.closedModel = closedModel;
        this.openModel = openModel;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return meshes[(Int32) info.Data & 0b00_0111].GetMeshData();
    }

    /// <inheritdoc />
    public Boolean IsConnectable(World world, Side side, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block || block.Block != this) return false;

        var orientation = (Orientation) (block.Data & 0b00_0011);

        return orientation switch
        {
            Orientation.North => side is Side.Left or Side.Right,
            Orientation.East => side is Side.Front or Side.Back,
            Orientation.South => side is Side.Left or Side.Right,
            Orientation.West => side is Side.Front or Side.Back,
            _ => false
        };
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        BlockModel closed = modelProvider.GetModel(closedModel);
        BlockModel open = modelProvider.GetModel(openModel);

        closed.OverwriteTexture(texture);
        open.OverwriteTexture(texture);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) closedModels =
            closed.CreateAllOrientations(rotateTopAndBottomTexture: false);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) openModels =
            open.CreateAllOrientations(rotateTopAndBottomTexture: false);

        for (UInt32 data = 0b00_0000; data <= 0b00_0111; data++)
        {
            var orientation = (Orientation) (data & 0b00_0011);
            Boolean isClosed = (data & 0b00_0100) == 0;

            BlockMesh mesh = orientation.Pick(isClosed ? closedModels : openModels).CreateMesh(textureIndexProvider);
            meshes.Add(mesh);

            volumes.Add(CreateVolume(data));
        }
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        Boolean isClosed = (data & 0b00_0100) == 0;

        return (Orientation) (data & 0b00_0011) switch
        {
            Orientation.North => NorthSouth(offset: 0.375f),
            Orientation.East => WestEast(offset: 0.625f),
            Orientation.South => NorthSouth(offset: 0.625f),
            Orientation.West => WestEast(offset: 0.375f),
            _ => NorthSouth(offset: 0.375f)
        };

        BoundingVolume NorthSouth(Single offset)
        {
            if (isClosed)
                return new BoundingVolume(
                    new Vector3d(x: 0.96875f, y: 0.71875f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f),
                    new BoundingVolume(
                        new Vector3d(x: 0.96875f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.03125f, y: 0.71875f, z: 0.5f),
                        new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.03125f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    // Moving parts.
                    new BoundingVolume(
                        new Vector3d(x: 0.75f, y: 0.71875f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.75f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.25f, y: 0.71875f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.25f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)));

            return new BoundingVolume(
                new Vector3d(x: 0.96875f, y: 0.71875f, z: 0.5f),
                new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f),
                new BoundingVolume(
                    new Vector3d(x: 0.96875f, y: 0.28125f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.03125f, y: 0.71875f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.03125f, y: 0.28125f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                // Moving parts.
                new BoundingVolume(
                    new Vector3d(x: 0.875f, y: 0.71875f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3d(x: 0.875f, y: 0.28125f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3d(x: 0.125f, y: 0.71875f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3d(x: 0.125f, y: 0.28125f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)));
        }

        BoundingVolume WestEast(Single offset)
        {
            if (isClosed)
                return new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.71875f, z: 0.96875f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.96875f),
                        new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.71875f, z: 0.03125f),
                        new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.03125f),
                        new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    // Moving parts.
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.71875f, z: 0.75f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.75f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.71875f, z: 0.25f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.25f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)));

            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.71875f, z: 0.96875f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f),
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.28125f, z: 0.96875f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.71875f, z: 0.03125f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.28125f, z: 0.03125f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                // Moving parts.
                new BoundingVolume(
                    new Vector3d(offset, y: 0.71875f, z: 0.875f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3d(offset, y: 0.28125f, z: 0.875f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3d(offset, y: 0.71875f, z: 0.125f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3d(offset, y: 0.28125f, z: 0.125f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)));
        }
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_0111];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Boolean connectX = CheckOrientation(world, position, Orientation.East) ||
                           CheckOrientation(world, position, Orientation.West);

        Boolean connectZ = CheckOrientation(world, position, Orientation.South) ||
                           CheckOrientation(world, position, Orientation.North);

        return connectX || connectZ;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Orientation orientation = actor?.Head.Forward.ToOrientation() ?? Orientation.North;

        Boolean connectX = CheckOrientation(world, position, Orientation.East) ||
                           CheckOrientation(world, position, Orientation.West);

        Boolean connectZ = CheckOrientation(world, position, Orientation.South) ||
                           CheckOrientation(world, position, Orientation.North);

        if (orientation.IsZ() && !connectX) orientation = orientation.Rotate();
        else if (orientation.IsX() && !connectZ) orientation = orientation.Rotate();

        world.SetBlock(this.AsInstance((UInt32) orientation), position);
    }

    private static Boolean CheckOrientation(World world, Vector3i position, Orientation orientation)
    {
        Vector3i otherPosition = orientation.Offset(position);

        return world.GetBlock(otherPosition)?.Block is IWideConnectable connectable &&
               connectable.IsConnectable(world, orientation.ToSide().Opposite(), otherPosition);
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        var orientation = (Orientation) (data & 0b00_0011);
        Boolean isClosed = (data & 0b00_0100) == 0;

        // Check if orientation has to be inverted.
        if (isClosed &&
            Vector2d.Dot(
                orientation.ToVector3().Xz,
                actor.Position.Xz - new Vector2(position.X + 0.5f, position.Z + 0.5f)) < 0)
            orientation = orientation.Opposite();

        Vector3d center = isClosed
            ? new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f) + -orientation.ToVector3() * 0.09375f
            : new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f);

        Single closedOffset = isClosed ? 0.09375f : 0f;

        Vector3d extents = orientation is Orientation.North or Orientation.South
            ? new Vector3d(x: 0.5f, y: 0.375f, 0.125f + closedOffset)
            : new Vector3d(0.125f + closedOffset, y: 0.375f, z: 0.5f);

        BoundingVolume volume = new(center, extents);

        if (actor.Collider.Intersects(volume.GetColliderAt(position))) return;

        actor.World.SetBlock(
            this.AsInstance((UInt32) ((isClosed ? 0b00_0100 : 0b00_0000) | (Int32) orientation.Opposite())),
            position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        var blockOrientation = (Orientation) (data & 0b00_0011);

        if (blockOrientation.Axis() != side.Axis().Rotate()) return;

        Boolean valid =
            CheckOrientation(world, position, side.ToOrientation()) ||
            CheckOrientation(world, position, side.ToOrientation().Opposite());

        if (!valid) Destroy(world, position);
    }
}
