﻿// <copyright file="PermeableBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A solid and full block that allows water flow through it. The block becomes darker in fluids.
///     Data bit usage: <c>------</c>
/// </summary>
public class PermeableBlock : BasicBlock, IFillable
{
    internal PermeableBlock(String name, String namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            layout) {}

    /// <inheritdoc />
    public Boolean IsInflowAllowed(World world, Vector3i position, Side side, Fluid fluid)
    {
        return fluid.Viscosity < 100;
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        return base.GetMeshData(info) with {Tint = info.Fluid.IsFluid ? ColorS.LightGray : ColorS.None};
    }
}
