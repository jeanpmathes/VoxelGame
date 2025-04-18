// <copyright file="MudBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that slows down entities.
/// </summary>
public class MudBlock : BasicBlock
{
    private readonly Single maxVelocity;

    internal MudBlock(String name, String namedID, TextureLayout layout, Single maxVelocity) :
        base(
            name,
            namedID,
            BlockFlags.Collider with {IsOpaque = true},
            layout)
    {
        this.maxVelocity = maxVelocity;
    }

    /// <inheritdoc />
    protected override void ActorCollision(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        actor.Velocity = MathTools.Clamp(actor.Velocity, min: -1f, maxVelocity);
    }
}
