﻿// <copyright file="FluidBarrierBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that lets fluids through but can be closed by interacting with it.
///     Data bit usage: <c>-----o</c>
/// </summary>
// o: open
public class FluidBarrierBlock : BasicBlock, IFillable, ICombustible
{
    private readonly TextureLayout open;
    private SideArray<Int32> openTextureIndices = null!;

    internal FluidBarrierBlock(String name, String namedID, TextureLayout closed, TextureLayout open) :
        base(
            name,
            namedID,
            BlockFlags.Basic with {IsInteractable = true},
            closed)
    {
        this.open = open;
    }

    /// <inheritdoc />
    public Boolean IsInflowAllowed(World world, Vector3i position, Side side, Fluid fluid)
    {
        if (fluid.IsGas) return true;

        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return (block.Data & 0b00_0001) == 1;
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        base.OnSetUp(textureIndexProvider, modelProvider, visuals);

        openTextureIndices = open.GetTextureIndices(textureIndexProvider, isBlock: true);
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        actor.World.SetBlock(this.AsInstance(data ^ 0b00_0001), position);
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        ISimple.MeshData mesh = base.GetMeshData(info);

        if ((info.Data & 0b00_0001) == 1)
            mesh = mesh with {TextureIndex = openTextureIndices[info.Side]};

        return mesh;
    }
}
