// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A plant made out of two intersecting planes. It uses neutral tint.
///     Data bit usage: <c>-----l</c>
/// </summary>
// l: lowered
public class CrossPlantBlock : Block, ICombustible, IFillable, IFoliage
{
    private readonly TID texture;
    private readonly ColorS tint;

    private readonly List<BlockMesh> meshes = [];

    /// <summary>
    ///     Initializes a new instance of a cross plant.
    /// </summary>
    /// <param name="name">The name of this block.</param>
    /// <param name="namedID">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The name of the texture of this block.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="boundingVolume">The bounding box of this block.</param>
    /// <param name="isTintNeutral">Whether the block is neutral tinted.</param>
    internal CrossPlantBlock(String name, String namedID, TID texture, BlockFlags flags, BoundingVolume boundingVolume, Boolean isTintNeutral = true) :
        base(
            name,
            namedID,
            flags with {IsFull = false, IsOpaque = false},
            boundingVolume)
    {
        this.texture = texture;

        tint = isTintNeutral ? ColorS.Neutral : ColorS.None;
    }

    IFoliage.MeshData IFoliage.GetMeshData(BlockMeshInfo info)
    {
        return new IFoliage.MeshData(meshes[(Int32) info.Data & 0b00_0001])
        {
            Tint = tint
        };
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Four) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        Int32 textureIndex = textureIndexProvider.GetTextureIndex(texture);

        for (var data = 0; data <= 0b00_0001; data++) meshes.Add(BlockMeshes.CreateCrossPlantMesh(visuals.FoliageQuality, textureIndex, (data & 0b1) != 0));
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return PlantBehaviour.CanPlace(world, position);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        PlantBehaviour.DoPlace(this, world, position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        PlantBehaviour.NeighborUpdate(world, this, position, side);
    }
}
