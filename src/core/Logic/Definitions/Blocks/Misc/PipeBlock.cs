﻿// <copyright file="PipeBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that connects to other pipes and allows water flow.
///     Data bit usage: <c>fblrdt</c>
/// </summary>
// f: front
// b: back
// l: left
// r: right
// d: bottom
// t: top
public class PipeBlock<TConnect> : Block, IFillable, IComplex where TConnect : IPipeConnectable
{
    private readonly float diameter;
    private readonly List<BlockMesh> meshes = new(capacity: 64);

    private readonly List<BoundingVolume> volumes = new();

    internal PipeBlock(string name, string namedID, float diameter, string centerModel, string connectorModel,
        string surfaceModel) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(diameter, diameter, diameter)))
    {
        this.diameter = diameter;

        BlockModel center = BlockModel.Load(centerModel);

        BlockModel frontConnector = BlockModel.Load(connectorModel);
        BlockModel frontSurface = BlockModel.Load(surfaceModel);

        (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
            connectors = frontConnector.CreateAllSides();

        (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
            surfaces = frontSurface.CreateAllSides();

        center.Lock();
        connectors.Lock();
        surfaces.Lock();

        for (uint data = 0b00_0000; data <= 0b11_1111; data++)
        {
            var sides = (BlockSides) data;

            BlockMesh mesh = BlockModel.GetCombinedMesh(
                center,
                BlockSide.Front.IsSet(sides) ? connectors.front : surfaces.front,
                BlockSide.Back.IsSet(sides) ? connectors.back : surfaces.back,
                BlockSide.Left.IsSet(sides) ? connectors.left : surfaces.left,
                BlockSide.Right.IsSet(sides) ? connectors.right : surfaces.right,
                BlockSide.Bottom.IsSet(sides) ? connectors.bottom : surfaces.bottom,
                BlockSide.Top.IsSet(sides) ? connectors.top : surfaces.top);

            meshes.Add(mesh);

            volumes.Add(CreateVolume(data));
        }
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh mesh = meshes[(int) info.Data];

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

    private BoundingVolume CreateVolume(uint data)
    {
        List<BoundingVolume> connectors = new(BitHelper.CountSetBits(data));

        double connectorWidth = (0.5 - diameter) / 2.0;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!side.IsSet((BlockSides) data)) continue;

            var direction = side.Direction().ToVector3d();

            connectors.Add(
                new BoundingVolume(
                    (0.5, 0.5, 0.5) + direction * (0.5 - connectorWidth),
                    (diameter, diameter, diameter) + direction.Absolute() * (connectorWidth - diameter)));
        }

        return new BoundingVolume(
            new Vector3d(x: 0.5, y: 0.5, z: 0.5),
            new Vector3d(diameter, diameter, diameter),
            connectors.ToArray());
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b11_1111];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        uint data = GetConnectionData(world, position);

        OpenOpposingSide(ref data);

        world.SetBlock(this.AsInstance(data), position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        uint updatedData = GetConnectionData(world, position);
        OpenOpposingSide(ref updatedData);

        if (updatedData != data) world.SetBlock(this.AsInstance(updatedData), position);
    }

    private uint GetConnectionData(World world, Vector3i position)
    {
        var sides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Vector3i otherPosition = side.Offset(position);
            BlockInstance? otherBlock = world.GetBlock(otherPosition);

            if (otherBlock?.Block == this || (otherBlock?.Block is TConnect connectable &&
                                              connectable.IsConnectable(world, side, otherPosition))) sides |= side.ToFlag();
        }

        return (uint) sides;
    }

    private static void OpenOpposingSide(ref uint data)
    {
        if (BitHelper.CountSetBits(data) != 1) return;

        if ((data & 0b11_0000) != 0) data = 0b11_0000;

        if ((data & 0b00_1100) != 0) data = 0b00_1100;

        if ((data & 0b00_0011) != 0) data = 0b00_0011;
    }

    private static bool IsSideOpen(World world, Vector3i position, BlockSide side)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return side.IsSet((BlockSides) block.Data);
    }
}
