// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A plant made out of two intersecting planes. It uses neutral tint.
///     Data bit usage: <c>-----l</c>
/// </summary>
// l: lowered
public class CrossPlantBlock : Block, ICombustible, IFillable, IFoliage
{
    private readonly string texture;

    private readonly List<BlockMesh> meshes = new();

    /// <summary>
    ///     Initializes a new instance of a cross plant.
    /// </summary>
    /// <param name="name">The name of this block.</param>
    /// <param name="namedID">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The name of the texture of this block.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="boundingVolume">The bounding box of this block.</param>
    internal CrossPlantBlock(string name, string namedID, string texture, BlockFlags flags,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            flags with {IsFull = false, IsOpaque = false},
            boundingVolume)
    {
        this.texture = texture;
    }

    IFoliage.MeshData IFoliage.GetMeshData(BlockMeshInfo info)
    {
        return new IFoliage.MeshData(meshes[(int) info.Data & 0b00_0001])
        {
            Tint = TintColor.Neutral
        };
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Four) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        int textureIndex = indexProvider.GetTextureIndex(texture);

        for (var data = 0; data <= 0b00_0001; data++) meshes.Add(BlockMeshes.CreateCrossPlantMesh(visuals.FoliageQuality, textureIndex, (data & 0b1) != 0));
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return PlantBehaviour.CanPlace(world, position);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        PlantBehaviour.DoPlace(this, world, position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        PlantBehaviour.NeighborUpdate(world, this, position, side);
    }
}
