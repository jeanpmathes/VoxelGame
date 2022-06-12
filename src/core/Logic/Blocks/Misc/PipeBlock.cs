// <copyright file="PipeBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Blocks;

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

    internal PipeBlock(string name, string namedId, float diameter, string centerModel, string connectorModel,
        string surfaceModel) :
        base(
            name,
            namedId,
            BlockFlags.Solid,
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
            BlockMesh mesh = BlockModel.GetCombinedMesh(
                center,
                BlockSide.Front.IsSet(data) ? connectors.front : surfaces.front,
                BlockSide.Back.IsSet(data) ? connectors.back : surfaces.back,
                BlockSide.Left.IsSet(data) ? connectors.left : surfaces.left,
                BlockSide.Right.IsSet(data) ? connectors.right : surfaces.right,
                BlockSide.Bottom.IsSet(data) ? connectors.bottom : surfaces.bottom,
                BlockSide.Top.IsSet(data) ? connectors.top : surfaces.top);

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
    public bool RenderFluid => false;

    /// <inheritdoc />
    public bool AllowInflow(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    public bool AllowOutflow(World world, Vector3i position, BlockSide side)
    {
        return IsSideOpen(world, position, side);
    }

    private BoundingVolume CreateVolume(uint data)
    {
        List<BoundingVolume> connectors = new(BitHelper.CountSetBits(data));

        double connectorWidth = (0.5 - diameter) / 2.0;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!side.IsSet(data)) continue;

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
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        uint updatedData = GetConnectionData(world, position);
        OpenOpposingSide(ref updatedData);

        if (updatedData != data) world.SetBlock(this.AsInstance(updatedData), position);
    }

    private uint GetConnectionData(World world, Vector3i position)
    {
        uint data = 0;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Vector3i otherPosition = side.Offset(position);
            BlockInstance? otherBlock = world.GetBlock(otherPosition);

            if (otherBlock?.Block == this || otherBlock?.Block is TConnect connectable &&
                connectable.IsConnectable(world, side, otherPosition)) data |= side.ToFlag();
        }

        return data;
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

        return side.IsSet(block.Data);
    }
}
