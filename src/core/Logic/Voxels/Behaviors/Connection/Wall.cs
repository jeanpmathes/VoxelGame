// <copyright file="Wall.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

/// <summary>
///     Provides the bounding volume for the <see cref="WideConnecting" /> wall block.
/// </summary>
public partial class Wall : BlockBehavior, IBehavior<Wall, BlockBehavior, Block>
{
    private readonly Connecting connecting;

    [Constructible]
    private Wall(Block subject) : base(subject)
    {
        subject.Require<WideConnecting>();
        connecting = subject.Require<Connecting>();

        subject.Require<Fillable>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);

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

        List<BoundingVolume> children = new(capacity: 6);


        if (north)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.125f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.125f)));
        }

        if (east)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.875f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.1875f)));
        }

        if (south)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.46875f, z: 0.875f),
                new Vector3d(x: 0.1875f, y: 0.46875f, z: 0.125f)));
        }

        if (west)
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.5f),
                new Vector3d(x: 0.125f, y: 0.46875f, z: 0.1875f)));

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.25f, y: 0.5f, z: 0.25f),
            children.ToArray());
    }
}
