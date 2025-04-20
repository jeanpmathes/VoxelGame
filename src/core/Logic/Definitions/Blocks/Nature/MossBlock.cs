// <copyright file="MossBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A flat block of moss.
/// </summary>
public class MossBlock : Block, IVaryingHeight, IFillable, ICombustible
{
    private const Int32 Height = 2;

    private readonly TID texture;

    private Int32 textureIndex;

    internal MossBlock(String name, String namedID, TID texture) : base(
        name,
        namedID,
        BlockFlags.Solid with {IsOpaque = true, IsReplaceable = true},
        BoundingVolume.BlockWithHeight(Height))
    {
        this.texture = texture;
    }

    /// <inheritdoc />
    public Int32 GetHeight(UInt32 data)
    {
        return Height;
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        return new IVaryingHeight.MeshData
        {
            TextureIndex = textureIndex,
            Tint = ColorS.None
        };
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        textureIndex = textureIndexProvider.GetTextureIndex(texture);
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Three)
            ScheduleDestroy(world, position);
    }
}
