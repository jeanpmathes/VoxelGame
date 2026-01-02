// <copyright file="Targeter.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Implements targeting functionality based on an actor's head position and orientation.
///     If an actor does not have a head, no targeting will be performed.
/// </summary>
public partial class Targeter : ActorComponent
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;

    [Constructible]
    private Targeter(Actor subject) : base(subject)
    {
        targeting = Subject.GetRequiredComponent<Targeting>();
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        Update();
    }

    /// <summary>
    ///     Update the targeting. This method will be called every logic update, but can be called manually if needed.
    /// </summary>
    public void Update()
    {
        World world = Subject.World;
        Transform? start = Subject.Head;

        if (start != null)
        {
            var ray = new Ray(start.Position, start.Forward, length: 6f);
            (Vector3i, Side)? hit = Raycast.CastBlockRay(world, ray);

            if (hit is var (hitPosition, hitSide) && world.GetContent(hitPosition) is var (block, fluid))
            {
                targeting.HasTarget = true;
                targeting.Position = hitPosition;
                targeting.Side = hitSide;
                targeting.Block = block;
                targeting.Fluid = fluid;

                return;
            }
        }

        targeting.HasTarget = false;
        targeting.Position = null;
        targeting.Side = Side.All;
        targeting.Block = null;
        targeting.Fluid = null;
    }
}
