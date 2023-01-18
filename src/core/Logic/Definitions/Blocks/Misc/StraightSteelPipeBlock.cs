// <copyright file="StraightSteelPipeBlock.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that only connects to steel pipes at specific sides.
///     Data bit usage: <c>----aa</c>
/// </summary>
// aa: axis
public class StraightSteelPipeBlock : Block, IFillable, IIndustrialPipeConnectable, IComplex
{
    private readonly float diameter;

    private readonly List<BlockMesh> meshes = new(capacity: 3);
    private readonly List<BoundingVolume> volumes = new();

    internal StraightSteelPipeBlock(string name, string namedId, float diameter, string model) :
        base(
            name,
            namedId,
            BlockFlags.Solid,
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(diameter, diameter, z: 0.5f)))
    {
        this.diameter = diameter;

        (BlockModel x, BlockModel y, BlockModel z) = BlockModel.Load(model).CreateAllAxis();

        meshes.Add(x.Mesh);
        meshes.Add(y.Mesh);
        meshes.Add(z.Mesh);

        for (uint data = 0; data <= 0b00_0011; data++)
        {
            if (data == 0b00_0011) continue; // End condition not changed to keep consistent with other blocks.

            volumes.Add(CreateVolume(data));
        }
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh mesh = meshes[(int) info.Data & 0b00_0011];

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

    /// <inheritdoc />
    public bool IsConnectable(World world, BlockSide side, Vector3i position)
    {
        return IsSideOpen(world, position, side);
    }

    private BoundingVolume CreateVolume(uint data)
    {
        var axis = (Axis) (data & 0b00_0011);

        return new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), axis.Vector3(onAxis: 0.5f, diameter));
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b00_0011];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        world.SetBlock(this.AsInstance((uint) (entity?.TargetSide ?? BlockSide.Front).Axis()), position);
    }

    private static bool IsSideOpen(World world, Vector3i position, BlockSide side)
    {
        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return side.Axis() == (Axis) (block.Data & 0b00_0011);
    }
}

