// <copyright file="SpiderWeb.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that slows down entities that collide with it.
///     Data bit usage: <c>------</c>
/// </summary>
public class SpiderWebBlock : CrossBlock, ICombustible, IFillable
{
    private readonly float maxVelocity;

    /// <summary>
    ///     Creates a SpiderWeb block, a block that slows down entities that collide with it.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedId">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The texture of this block.</param>
    /// <param name="maxVelocity">The maximum velocity of entities colliding with this block.</param>
    internal SpiderWebBlock(string name, string namedId, string texture, float maxVelocity) :
        base(
            name,
            namedId,
            texture,
            BlockFlags.Trigger,
            BoundingVolume.CrossBlock)
    {
        this.maxVelocity = maxVelocity;
    }

    /// <inheritdoc />
    public void FluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid.IsFluid) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void EntityCollision(PhysicsEntity entity, Vector3i position, uint data)
    {
        entity.Velocity = VMath.Clamp(entity.Velocity, min: -1f, maxVelocity);
    }
}

