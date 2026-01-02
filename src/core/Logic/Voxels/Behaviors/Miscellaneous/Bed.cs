// <copyright file="Bed.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;

/// <summary>
///     Implements basic functionality for beds.
/// </summary>
public partial class Bed : BlockBehavior, IBehavior<Bed, BlockBehavior, Block>
{
    private readonly Composite composite;
    private readonly LateralRotatable rotatable;

    [Constructible]
    private Bed(Block subject) : base(subject)
    {
        rotatable = subject.Require<LateralRotatable>();
        composite = subject.Require<Composite>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
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

    private static void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        message.World.SpawnPosition = new Vector3d(message.Position.X, message.Position.Y + 1f, message.Position.Z);
    }
}
