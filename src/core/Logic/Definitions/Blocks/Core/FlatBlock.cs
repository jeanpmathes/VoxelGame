﻿// <copyright file="FlatBlock.cs" company="VoxelGame">
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
///     This class represents a block with a single face that sticks to other blocks.
///     Data bit usage: <c>----oo</c>
/// </summary>
// o: orientation
public class FlatBlock : Block, IFillable, IComplex
{
    private static readonly float[][] sideVertices =
    {
        CreateSide(Orientation.North),
        CreateSide(Orientation.East),
        CreateSide(Orientation.South),
        CreateSide(Orientation.West)
    };

    private readonly float climbingVelocity;
    private readonly float slidingVelocity;

    private readonly string texture;

    private readonly List<BoundingVolume> volumes = new();

    private uint[] indices = null!;

    private int[] textureIndices = null!;

    /// <summary>
    ///     Creates a FlatBlock, a block with a single face that sticks to other blocks. It allows entities to climb and can
    ///     use neutral tints.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedId">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The texture to use for the block.</param>
    /// <param name="climbingVelocity">The velocity of players climbing the block.</param>
    /// <param name="slidingVelocity">The velocity of players sliding along the block.</param>
    internal FlatBlock(string name, string namedId, string texture, float climbingVelocity, float slidingVelocity) :
        base(
            name,
            namedId,
            BlockFlags.Trigger,
            BoundingVolume.Block)
    {
        this.climbingVelocity = climbingVelocity;
        this.slidingVelocity = slidingVelocity;

        this.texture = texture;

        for (uint data = 0; data <= 0b00_0011; data++)
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
        return this.AsInstance((uint) orientation);
    }

    private static float[] CreateSide(Orientation orientation)
    {
        return BlockModels.CreateFlatModel(orientation.ToBlockSide().Opposite(), offset: 0.01f);
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        indices = BlockModels.GenerateIndexDataArray(faces: 2);
        textureIndices = BlockModels.GenerateTextureDataArray(indexProvider.GetTextureIndex(texture), length: 8);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) (data & 0b00_0011)];
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        BlockSide side = entity?.TargetSide ?? BlockSide.Front;

        if (!side.IsLateral()) side = BlockSide.Back;
        var orientation = side.ToOrientation();

        return world.GetBlock(orientation.Opposite().Offset(position))?.IsSolidAndFull ?? false;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        BlockSide side = entity?.TargetSide ?? BlockSide.Front;
        if (!side.IsLateral()) side = BlockSide.Back;
        world.SetBlock(this.AsInstance((uint) side.ToOrientation()), position);
    }

    /// <inheritdoc />
    protected override void EntityCollision(PhysicsEntity entity, Vector3i position, uint data)
    {
        Vector3d forwardMovement = Vector3d.Dot(entity.Movement, entity.Forward) * entity.Forward;
        Vector3d newVelocity;

        if (forwardMovement.LengthSquared > 0.1f &&
            (Orientation) (data & 0b00_0011) == (-forwardMovement).ToOrientation())
        {
            float yVelocity = Vector3d.CalculateAngle(entity.LookingDirection, Vector3d.UnitY) < MathHelper.PiOver2
                ? climbingVelocity
                : -climbingVelocity;

            newVelocity = new Vector3d(entity.Velocity.X, yVelocity, entity.Velocity.Z);
        }
        else
        {
            newVelocity = new Vector3d(
                entity.Velocity.X,
                MathHelper.Clamp(entity.Velocity.Y, -slidingVelocity, double.MaxValue),
                entity.Velocity.Z);
        }

        entity.Velocity = newVelocity;
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        CheckBack(world, position, side, (Orientation) (data & 0b00_0011), schedule: false);
    }


    private protected void CheckBack(World world, Vector3i position, BlockSide side, Orientation blockOrientation,
        bool schedule)
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
        return IComplex.CreateData(vertexCount: 8, sideVertices[info.Data & 0b00_0011], textureIndices, indices);
    }
}

