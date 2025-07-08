// <copyright file="SpiderWeb.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using Body = VoxelGame.Core.Actors.Components.Body;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A block that slows down entities that collide with it.
///     Data bit usage: <c>------</c>
/// </summary>
public class SpiderWebBlock : CrossBlock, ICombustible
{
    private readonly Double maxVelocity;

    /// <summary>
    ///     Creates a SpiderWeb block, a block that slows down entities that collide with it.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The texture of this block.</param>
    /// <param name="maxVelocity">The maximum velocity of entities colliding with this block.</param>
    internal SpiderWebBlock(String name, String namedID, TID texture, Double maxVelocity) :
        base(
            name,
            namedID,
            texture,
            BlockFlags.Trigger,
            BoundingVolume.CrossBlock())
    {
        this.maxVelocity = maxVelocity;
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void ActorCollision(Body body, Vector3i position, UInt32 data)
    {
        body.Velocity = MathTools.Clamp(body.Velocity, min: -1f, maxVelocity);
    }
}
