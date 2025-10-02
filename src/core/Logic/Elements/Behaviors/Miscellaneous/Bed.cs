// <copyright file="Bed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Miscellaneous;

/// <summary>
///     Implements basic functionality for beds.
/// </summary>
public class Bed : BlockBehavior, IBehavior<Bed, BlockBehavior, Block>
{
    private readonly Composite composite;
    private readonly LateralRotatable rotatable;

    private Bed(Block subject) : base(subject)
    {
        rotatable = subject.Require<LateralRotatable>();
        composite = subject.Require<Composite>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    /// <inheritdoc />
    public static Bed Construct(Block input)
    {
        return new Bed(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Boolean isBase = composite.GetPartPosition(state).Z == 0;
        Orientation orientation = rotatable.GetOrientation(state);

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

            default: throw Exceptions.UnsupportedEnumValue(orientation);
        }

        return new BoundingVolume(
            new Vector3d(x: 0.5, y: 0.3125, z: 0.5),
            new Vector3d(x: 0.5, y: 0.125, z: 0.5),
            legs);
    }

    private void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        message.World.SpawnPosition = new Vector3d(message.Position.X, message.Position.Y + 1f, message.Position.Z);
    }
}
