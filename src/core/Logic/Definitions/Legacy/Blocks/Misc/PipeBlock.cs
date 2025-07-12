// <copyright file="PipeBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

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
    private readonly TID? texture;
    private readonly RID centerModel;
    private readonly RID connectorModel;
    private readonly RID surfaceModel;

    private readonly Single diameter;
    private readonly List<BlockMesh> meshes = new(capacity: 64);

    private readonly List<BoundingVolume> volumes = [];

    internal PipeBlock(String name, String namedID, Single diameter, TID? texture,
        RID centerModel, RID connectorModel, RID surfaceModel) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(diameter, diameter, diameter)))
    {
        this.diameter = diameter;

        this.texture = texture;
        this.centerModel = centerModel;
        this.connectorModel = connectorModel;
        this.surfaceModel = surfaceModel;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh mesh = meshes[(Int32) info.Data];

        return mesh.GetMeshData();
    }

    /// <inheritdoc />
    public Boolean IsFluidRendered => false;

    /// <inheritdoc />
    public Boolean IsInflowAllowed(World world, Vector3i position, Side side, Fluid fluid)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    public Boolean IsOutflowAllowed(World world, Vector3i position, Side side)
    {
        return IsSideOpen(world, position, side);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        BlockModel center = modelProvider.GetModel(centerModel);

        BlockModel frontConnector = modelProvider.GetModel(connectorModel);
        BlockModel frontSurface = modelProvider.GetModel(surfaceModel);

        if (texture is {} newTexture)
        {
            center.OverwriteTexture(newTexture, index: 0);
            frontConnector.OverwriteTexture(newTexture, index: 0);
            frontSurface.OverwriteTexture(newTexture, index: 0);
        }

        (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
            connectors = frontConnector.CreateAllSides();

        (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
            surfaces = frontSurface.CreateAllSides();

        center.Lock(textureIndexProvider);
        connectors.Lock(textureIndexProvider);
        surfaces.Lock(textureIndexProvider);

        for (UInt32 data = 0b00_0000; data <= 0b11_1111; data++)
        {
            var sides = (Sides) data;

            BlockMesh mesh = BlockModel.GetCombinedMesh(textureIndexProvider,
                center,
                sides.HasFlag(Sides.Front) ? connectors.front : surfaces.front,
                sides.HasFlag(Sides.Back) ? connectors.back : surfaces.back,
                sides.HasFlag(Sides.Left) ? connectors.left : surfaces.left,
                sides.HasFlag(Sides.Right) ? connectors.right : surfaces.right,
                sides.HasFlag(Sides.Bottom) ? connectors.bottom : surfaces.bottom,
                sides.HasFlag(Sides.Top) ? connectors.top : surfaces.top);

            meshes.Add(mesh);

            volumes.Add(CreateVolume(data));
        }
    }

    private BoundingVolume CreateVolume(UInt32 data)
    {
        List<BoundingVolume> connectors = new(BitTools.CountSetBits(data));

        Double connectorWidth = (0.5 - diameter) / 2.0;

        foreach (Side side in Side.All.Sides())
        {
            if (!((Sides) data).HasFlag(side.ToFlag())) continue;

            var direction = (Vector3d) side.Direction();

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
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b11_1111];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, Actor? actor)
    {
        UInt32 data = GetConnectionData(world, position);

        OpenOpposingSide(ref data);

        world.SetBlock(this.AsInstance(data), position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        UInt32 updatedData = GetConnectionData(world, position);
        OpenOpposingSide(ref updatedData);

        if (updatedData != data) world.SetBlock(this.AsInstance(updatedData), position);
    }

    private UInt32 GetConnectionData(World world, Vector3i position)
    {
        var sides = Sides.None;

        foreach (Side side in Side.All.Sides())
        {
            Vector3i otherPosition = side.Offset(position);
            BlockInstance? otherBlock = world.GetBlock(otherPosition);

            if (otherBlock?.Block == this || (otherBlock?.Block is TConnect connectable &&
                                              connectable.IsConnectable(world, side, otherPosition))) sides |= side.ToFlag();
        }

        return (UInt32) sides;
    }

    private static void OpenOpposingSide(ref UInt32 data)
    {
        if (BitTools.CountSetBits(data) != 1) return;

        if ((data & 0b11_0000) != 0) data = 0b11_0000;

        if ((data & 0b00_1100) != 0) data = 0b00_1100;

        if ((data & 0b00_0011) != 0) data = 0b00_0011;
    }

    private static Boolean IsSideOpen(World world, Vector3i position, Side side)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return ((Sides) block.Data).HasFlag(side.ToFlag());
    }
}
