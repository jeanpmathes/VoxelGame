﻿// <copyright file="DirtBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A simple block which allows the spread of grass.
///     Data bit usage: <c>------</c>
/// </summary>
public class DirtBlock : BasicBlock, IPlantable, IGrassSpreadable, IFillable
{
    private readonly TextureLayout wet;
    private int[] wetTextureIndices = null!;

    internal DirtBlock(string name, string namedId, TextureLayout normal, TextureLayout wet) :
        base(
            name,
            namedId,
            BlockFlags.Basic,
            normal)
    {
        this.wet = wet;
    }

    /// <inheritdoc />
    public bool AllowInflow(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.Viscosity < 100;
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        base.OnSetup(indexProvider);

        wetTextureIndices = wet.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        ISimple.MeshData mesh = base.GetMeshData(info);

        if (info.Fluid.IsFluid)
            mesh = mesh with {TextureIndex = wetTextureIndices[(int) info.Side]};

        return mesh;
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, uint data)
    {
        FluidInstance? potentialFluid = world.GetFluid(position);

        if (potentialFluid is not {} fluid) return;

        if (fluid.Fluid == Logic.Fluids.Instance.Water && fluid.Level == FluidLevel.Eight)
            world.SetBlock(Logic.Blocks.Instance.Mud.AsInstance(), position);
    }

    /// <inheritdoc />
    public override Content GenerateUpdate(Content content)
    {
        (BlockInstance block, FluidInstance fluid) = content;

        if (fluid.Fluid == Logic.Fluids.Instance.Water && fluid.Level == FluidLevel.Eight)
            return new Content(Logic.Blocks.Instance.Mud.AsInstance(), fluid);

        return new Content(block, fluid);
    }
}
