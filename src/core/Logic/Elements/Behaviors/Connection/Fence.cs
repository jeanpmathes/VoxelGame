// <copyright file="Fence.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
///     Provides the bounding volume for the <see cref="WideConnecting" /> fence block.
/// </summary>
public class Fence : BlockBehavior, IBehavior<Fence, BlockBehavior, Block>
{
    private readonly Connecting connecting;

    private Fence(Block subject) : base(subject)
    {
        subject.Require<WideConnecting>();
        connecting = subject.Require<Connecting>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    /// <inheritdoc />
    public static Fence Construct(Block input)
    {
        return new Fence(input);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);

        List<BoundingVolume> children = new(capacity: 6);

        if (north)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.28125f, z: 0.15625f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f)));

            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.71875f, z: 0.15625f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f)));
        }

        if (east)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.84375f, y: 0.28125f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f)));

            children.Add(new BoundingVolume(
                new Vector3d(x: 0.84375f, y: 0.71875f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f)));
        }

        if (south)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.28125f, z: 0.84375f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f)));

            children.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.71875f, z: 0.84375f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f)));
        }

        if (west)
        {
            children.Add(new BoundingVolume(
                new Vector3d(x: 0.15625f, y: 0.28125f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f)));

            children.Add(new BoundingVolume(
                new Vector3d(x: 0.15625f, y: 0.71875f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f)));
        }

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.1875f, y: 0.5f, z: 0.1875f),
            children.ToArray());
    }
}
