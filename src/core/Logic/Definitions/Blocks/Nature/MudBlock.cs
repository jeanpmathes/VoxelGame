// <copyright file="MudBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that slows down entities.
/// </summary>
public class MudBlock : BasicBlock, IFillable
{
    private readonly float maxVelocity;

    internal MudBlock(string name, string namedID, TextureLayout layout, float maxVelocity) :
        base(
            name,
            namedID,
            BlockFlags.Collider with {IsOpaque = true},
            layout)
    {
        this.maxVelocity = maxVelocity;
    }

    /// <inheritdoc />
    public bool IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.Viscosity < 200;
    }

    /// <inheritdoc />
    protected override void ActorCollision(PhysicsActor actor, Vector3i position, uint data)
    {
        actor.Velocity = VMath.Clamp(actor.Velocity, min: -1f, maxVelocity);
    }
}
