﻿// <copyright file="PumpBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     Pumps water upwards when interacted with.
///     Data bit usage: <c>------</c>
/// </summary>
internal class PumpBlock : BasicBlock, IIndustrialPipeConnectable, IFillable
{
    private readonly int pumpDistance;

    internal PumpBlock(string name, string namedID, int pumpDistance, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Basic with {IsInteractable = true},
            layout)
    {
        this.pumpDistance = pumpDistance;
    }

    public bool IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return side != BlockSide.Top;
    }

    public bool IsOutflowAllowed(World world, Vector3i position, BlockSide side)
    {
        return side == BlockSide.Top;
    }

    protected override void ActorInteract(PhysicsActor actor, Vector3i position, uint data)
    {
        Fluid.Elevate(actor.World, position, pumpDistance);
    }
}
