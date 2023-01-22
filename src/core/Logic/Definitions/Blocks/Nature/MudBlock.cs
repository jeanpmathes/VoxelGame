// <copyright file="MudBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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

    internal MudBlock(string name, string namedId, TextureLayout layout, float maxVelocity) :
        base(
            name,
            namedId,
            BlockFlags.Collider with {IsOpaque = true},
            layout)
    {
        this.maxVelocity = maxVelocity;
    }

    /// <inheritdoc />
    public bool AllowInflow(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.Viscosity < 200;
    }

    /// <inheritdoc />
    protected override void EntityCollision(PhysicsEntity entity, Vector3i position, uint data)
    {
        entity.Velocity = VMath.Clamp(entity.Velocity, min: -1f, maxVelocity);
    }
}

