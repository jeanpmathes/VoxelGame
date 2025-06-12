// <copyright file="LooseSnowBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A loose snow block.
/// </summary>
public class LooseSnowBlock : SnowBlock
{
    private readonly Double maxVelocity;

    internal LooseSnowBlock(String name, String namedID, TextureLayout layout, Double maxVelocity) : base(name, namedID, layout, isSolid: false, isTrigger: true)
    {
        this.maxVelocity = maxVelocity;
    }

    /// <inheritdoc />
    protected override void ActorCollision(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        Vector3d clamped = MathTools.Clamp(actor.Velocity, min: -1f, maxVelocity);
        Double height = GetHeight(data) / (Double) IHeightVariable.MaximumHeight;

        actor.Velocity = Vector3d.Lerp(actor.Velocity, clamped, height);
    }
}
