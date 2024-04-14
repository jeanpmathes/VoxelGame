﻿// <copyright file="StraightSteelPipeBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that only connects to steel pipes at specific sides.
///     Data bit usage: <c>----aa</c>
/// </summary>
// aa: axis
public class StraightSteelPipeBlock : Block, IFillable, IIndustrialPipeConnectable, IComplex
{
    private readonly Single diameter;

    private readonly List<BlockMesh> meshes = new(capacity: 3);
    private readonly List<BoundingVolume> volumes = [];

    internal StraightSteelPipeBlock(String name, String namedID, Single diameter, String model) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(diameter, diameter, z: 0.5f)))
    {
        this.diameter = diameter;

        (BlockModel x, BlockModel y, BlockModel z) = BlockModel.Load(model).CreateAllAxis();

        meshes.Add(x.Mesh);
        meshes.Add(y.Mesh);
        meshes.Add(z.Mesh);

        for (UInt32 data = 0; data <= 0b00_0011; data++)
        {
            if (data == 0b00_0011) continue; // End condition not changed to keep consistent with other blocks.

            volumes.Add(CreateVolume(data));
        }
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh mesh = meshes[(Int32) info.Data & 0b00_0011];

        return mesh.GetMeshData();
    }

    /// <inheritdoc />
    public Boolean IsFluidRendered => false;

    /// <inheritdoc />
    public Boolean IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    public Boolean IsOutflowAllowed(World world, Vector3i position, BlockSide side)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    public Boolean IsConnectable(World world, BlockSide side, Vector3i position)
    {
        return IsSideOpen(world, position, side);
    }

    private BoundingVolume CreateVolume(UInt32 data)
    {
        var axis = (Axis) (data & 0b00_0011);

        return new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), axis.Vector3(onAxis: 0.5f, diameter));
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_0011];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance((UInt32) (actor?.TargetSide ?? BlockSide.Front).Axis()), position);
    }

    private static Boolean IsSideOpen(World world, Vector3i position, BlockSide side)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return side.Axis() == (Axis) (block.Data & 0b00_0011);
    }
}
