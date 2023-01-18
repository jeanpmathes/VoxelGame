// <copyright file="PermeableBlock.cs" company="VoxelGame">
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
///     A solid and full block that allows water flow through it. The become darker in fluids.
///     Data bit usage: <c>------</c>
/// </summary>
public class PermeableBlock : BasicBlock, IFillable
{
    internal PermeableBlock(string name, string namedId, TextureLayout layout) :
        base(
            name,
            namedId,
            BlockFlags.Basic,
            layout) {}

    /// <inheritdoc />
    public bool AllowInflow(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.Viscosity < 100;
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        return base.GetMeshData(info) with {Tint = info.Fluid.IsFluid ? TintColor.LightGray : TintColor.None};
    }
}

