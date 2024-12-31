// <copyright file="FlatBlock.cs" company="VoxelGame">
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
///     This class represents a block with a single face that sticks to other blocks.
///     Data bit usage: <c>----oo</c>
/// </summary>
// o: orientation
public class FlatBlock : Block, IFillable, IComplex
{
    private readonly TID texture;

    private readonly Single climbingVelocity;
    private readonly Single slidingVelocity;

    private readonly List<BlockMesh> meshes = [];
    private readonly List<BoundingVolume> volumes = [];


    /// <summary>
    ///     Creates a FlatBlock, a block with a single face that sticks to other blocks. It allows entities to climb and can
    ///     use neutral tints.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The texture to use for the block.</param>
    /// <param name="climbingVelocity">The velocity of players climbing the block.</param>
    /// <param name="slidingVelocity">The velocity of players sliding along the block.</param>
    internal FlatBlock(String name, String namedID, TID texture, Single climbingVelocity, Single slidingVelocity) :
        base(
            name,
            namedID,
            BlockFlags.Trigger,
            BoundingVolume.Block)
    {
        this.climbingVelocity = climbingVelocity;
        this.slidingVelocity = slidingVelocity;

        this.texture = texture;

        for (UInt32 data = 0; data <= 0b00_0011; data++)
        {
            BoundingVolume volume = (Orientation) (data & 0b00_0011) switch
            {
                Orientation.North => new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.95f),
                    new Vector3d(x: 0.45f, y: 0.5f, z: 0.05f)),
                Orientation.South => new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.05f),
                    new Vector3d(x: 0.45f, y: 0.5f, z: 0.05f)),
                Orientation.West => new BoundingVolume(
                    new Vector3d(x: 0.95f, y: 0.5f, z: 0.5f),
                    new Vector3d(x: 0.05f, y: 0.5f, z: 0.45f)),
                Orientation.East => new BoundingVolume(
                    new Vector3d(x: 0.05f, y: 0.5f, z: 0.5f),
                    new Vector3d(x: 0.05f, y: 0.5f, z: 0.45f)),
                _ => new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.95f),
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.05f))
            };

            volumes.Add(volume);
        }
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return GetMeshData(info);
    }

    /// <summary>
    ///     Get an instance of this block with the given orientation.
    /// </summary>
    /// <param name="orientation">
    ///     The orientation of the block. The block will look in that direction, meaning it is placed on
    ///     the wall on the opposite side.
    /// </param>
    /// <returns>The block with the given orientation.</returns>
    public BlockInstance GetInstance(Orientation orientation)
    {
        return this.AsInstance((UInt32) orientation);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        Int32 textureIndex = textureIndexProvider.GetTextureIndex(texture);

        foreach (Orientation orientation in Orientations.All)
        {
            BlockMesh mesh = BlockMeshes.CreateFlatModel(
                orientation.ToSide().Opposite(),
                offset: 0.01f,
                textureIndex);

            meshes.Add(mesh);
        }
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) (data & 0b00_0011)];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Side side = actor?.TargetSide ?? Side.Front;

        if (!side.IsLateral()) side = Side.Back;
        var orientation = side.ToOrientation();

        return world.GetBlock(orientation.Opposite().Offset(position))?.IsSolidAndFull ?? false;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Side side = actor?.TargetSide ?? Side.Front;
        if (!side.IsLateral()) side = Side.Back;
        world.SetBlock(this.AsInstance((UInt32) side.ToOrientation()), position);
    }

    /// <inheritdoc />
    protected override void ActorCollision(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        Vector3d forwardMovement = Vector3d.Dot(actor.Movement, actor.Forward) * actor.Forward;
        Vector3d newVelocity;

        if (forwardMovement.LengthSquared > 0.1f &&
            (Orientation) (data & 0b00_0011) == (-forwardMovement).ToOrientation())
        {
            Single yVelocity = Vector3d.CalculateAngle(actor.Head.Forward, Vector3d.UnitY) < MathHelper.PiOver2
                ? climbingVelocity
                : -climbingVelocity;

            newVelocity = new Vector3d(actor.Velocity.X, yVelocity, actor.Velocity.Z);
        }
        else
        {
            newVelocity = new Vector3d(
                actor.Velocity.X,
                MathHelper.Clamp(actor.Velocity.Y, -slidingVelocity, Double.MaxValue),
                actor.Velocity.Z);
        }

        actor.Velocity = newVelocity;
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        CheckBack(world, position, side, (Orientation) (data & 0b00_0011), schedule: false);
    }


    private protected void CheckBack(World world, Vector3i position, Side side, Orientation blockOrientation,
        Boolean schedule)
    {
        if (!side.IsLateral()) return;

        if (blockOrientation != side.ToOrientation().Opposite() ||
            (world.GetBlock(blockOrientation.Opposite().Offset(position))?.IsSolidAndFull ?? false)) return;

        if (schedule) ScheduleDestroy(world, position);
        else Destroy(world, position);
    }

    /// <summary>
    ///     Override this method to create custom mesh data for this block.
    /// </summary>
    protected virtual IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        var meshIndex = (Int32) (info.Data & 0b00_0011);

        return meshes[meshIndex].GetMeshData();
    }
}
