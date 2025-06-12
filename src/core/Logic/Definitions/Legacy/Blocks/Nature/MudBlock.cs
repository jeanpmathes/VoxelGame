// <copyright file="MudBlock.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A block that slows down entities.
/// </summary>
public class MudBlock : BasicBlock, IPlantable
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
    public Boolean TryGrow(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid != Elements.Fluids.Instance.FreshWater)
            return false;

        FluidLevel remaining = FluidLevel.Eight - (Int32) level;

        world.SetContent(remaining >= FluidLevel.One
                ? new Content(Elements.Legacy.Blocks.Instance.Dirt.AsInstance(), Elements.Fluids.Instance.FreshWater.AsInstance(remaining))
                : new Content(Elements.Legacy.Blocks.Instance.Dirt),
            position);

        return true;
    }

    /// <inheritdoc />
    protected override void ActorCollision(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        actor.Velocity = MathTools.Clamp(actor.Velocity, min: -1f, maxVelocity);
    }
}
