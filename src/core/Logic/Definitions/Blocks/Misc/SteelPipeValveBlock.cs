// <copyright file="SteelPipeValveBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
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
    private readonly RID openModel;
    private readonly RID closedModel;

    private readonly Single diameter;

    private readonly List<BlockMesh?> meshes = new(capacity: 8);
    private readonly List<BoundingVolume> volumes = [];

    internal SteelPipeValveBlock(String name, String namedID, Single diameter,
        RID openModel, RID closedModel) :
        base(
            name,
            namedID,
            BlockFlags.Functional with {IsOpaque = true},
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(diameter, diameter, z: 0.5f)))
    {
        this.diameter = diameter;

        this.openModel = openModel;
        this.closedModel = closedModel;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh? mesh = meshes[(Int32) info.Data & 0b00_0111];
        Debug.Assert(mesh != null);

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
    public Boolean IsConnectable(World world, Side side, Vector3i position)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return side.Axis() == (Axis) (block.Data & 0b00_0011);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        (BlockModel openX, BlockModel openY, BlockModel openZ) = modelProvider.GetModel(openModel).CreateAllAxis();
        (BlockModel closedX, BlockModel closedY, BlockModel closedZ) = modelProvider.GetModel(closedModel).CreateAllAxis();

        meshes.Add(openX.CreateMesh(textureIndexProvider));
        meshes.Add(openY.CreateMesh(textureIndexProvider));
        meshes.Add(openZ.CreateMesh(textureIndexProvider));
        meshes.Add(item: null);

        meshes.Add(closedX.CreateMesh(textureIndexProvider));
        meshes.Add(closedY.CreateMesh(textureIndexProvider));
        meshes.Add(closedZ.CreateMesh(textureIndexProvider));
        meshes.Add(item: null);

        for (UInt32 data = 0; data <= 0b00_0111; data++) volumes.Add((data & 0b00_0011) == 0b11 ? null! : CreateVolume(data));
    }

    private BoundingVolume CreateVolume(UInt32 data)
    {
        var axis = (Axis) (data & 0b00_0011);

        return new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), axis.Vector3(onAxis: 0.5f, diameter));
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) (data & 0b00_0111)];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance((UInt32) (actor?.TargetSide ?? Side.Front).Axis()), position);
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        actor.World.SetBlock(this.AsInstance(data ^ 0b00_0100), position);
    }

    private static Boolean IsSideOpen(World world, Vector3i position, Side side)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        if ((block.Data & 0b00_0100) != 0) return false;

        return side.Axis() == (Axis) (block.Data & 0b00_0011);
    }
}
