// <copyright file="Targeting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Offers targeting functionality for actors, allowing them to target blocks and fluids in the world.
/// </summary>
public class Targeting
{
    private Target? target;

    /// <summary>
    ///     Whether this class is currently targeting something.
    /// </summary>
    public Boolean HasTarget => target != null;

    /// <summary>
    ///     The targeted position, or null if no position is targeted.
    /// </summary>
    public Vector3i? Position => target?.position;

    /// <summary>
    ///     The targeted side, or <see cref="BlockSide.All" /> if no side is targeted.
    /// </summary>
    public BlockSide Side => target?.side ?? BlockSide.All;

    /// <summary>
    ///     The targeted block, or null if no block is targeted.
    /// </summary>
    public BlockInstance? Block => target?.block;

    /// <summary>
    ///     The targeted fluid, or null if no fluid is targeted.
    /// </summary>
    public FluidInstance? Fluid => target?.fluid;

    /// <summary>
    ///     Update the target.
    /// </summary>
    /// <param name="start">From which orientable the targeting should start.</param>
    /// <param name="world">The world to target in.</param>
    public void Update(IOrientable start, World world)
    {
        var ray = new Ray(start.Position, start.Forward, length: 6f);
        (Vector3i, BlockSide)? hit = Raycast.CastBlockRay(world, ray);

        if (hit is var (hitPosition, hitSide) && world.GetContent(hitPosition) is var (block, fluid))
        {
            target ??= new Target();

            target.position = hitPosition;
            target.side = hitSide;
            target.block = block;
            target.fluid = fluid;
        }
        else
        {
            target = null;
        }
    }

    private sealed class Target
    {
        public Vector3i position;
        public BlockSide side;
        public BlockInstance block;
        public FluidInstance fluid;
    }
}
