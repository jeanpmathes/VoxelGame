﻿// <copyright file="SteelPipeValveBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that only connects to steel pipes at specific sides and can be closed.
///     Data bit usage: <c>---oaa</c>
/// </summary>
// aa: axis
// o: open
public class SteelPipeValveBlock : Block, IFillable, IIndustrialPipeConnectable, IComplex
{
    private readonly float diameter;
    private readonly List<BlockMesh?> meshes = new(capacity: 8);

    private readonly List<BoundingVolume> volumes = [];

    internal SteelPipeValveBlock(string name, string namedID, float diameter, string openModel,
        string closedModel) :
        base(
            name,
            namedID,
            BlockFlags.Functional with {IsOpaque = true},
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(diameter, diameter, z: 0.5f)))
    {
        this.diameter = diameter;

        (BlockModel openX, BlockModel openY, BlockModel openZ) = BlockModel.Load(openModel).CreateAllAxis();
        (BlockModel closedX, BlockModel closedY, BlockModel closedZ) = BlockModel.Load(closedModel).CreateAllAxis();

        meshes.Add(openX.Mesh);
        meshes.Add(openY.Mesh);
        meshes.Add(openZ.Mesh);
        meshes.Add(item: null);

        meshes.Add(closedX.Mesh);
        meshes.Add(closedY.Mesh);
        meshes.Add(closedZ.Mesh);
        meshes.Add(item: null);

        for (uint data = 0; data <= 0b00_0111; data++)
        {
            volumes.Add((data & 0b00_0011) == 0b11 ? null! : CreateVolume(data));
        }
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh? mesh = meshes[(int) info.Data & 0b00_0111];
        Debug.Assert(mesh != null);

        return mesh.GetMeshData();
    }

    /// <inheritdoc />
    public bool IsFluidRendered => false;

    /// <inheritdoc />
    public bool IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    public bool IsOutflowAllowed(World world, Vector3i position, BlockSide side)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    public bool IsConnectable(World world, BlockSide side, Vector3i position)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return side.Axis() == (Axis) (block.Data & 0b00_0011);
    }

    private BoundingVolume CreateVolume(uint data)
    {
        var axis = (Axis) (data & 0b00_0011);

        return new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), axis.Vector3(onAxis: 0.5f, diameter));
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) (data & 0b00_0111)];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance((uint) (actor?.TargetSide ?? BlockSide.Front).Axis()), position);
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, uint data)
    {
        actor.World.SetBlock(this.AsInstance(data ^ 0b00_0100), position);
    }

    private static bool IsSideOpen(World world, Vector3i position, BlockSide side)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        if ((block.Data & 0b00_0100) != 0) return false;

        return side.Axis() == (Axis) (block.Data & 0b00_0011);
    }
}
