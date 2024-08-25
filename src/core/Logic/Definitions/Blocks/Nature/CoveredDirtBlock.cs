// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that changes into dirt when something is placed on top of it. This block can use a neutral tint if
///     specified in the constructor.
///     Data bit usage: <c>------</c>
/// </summary>
public class CoveredDirtBlock : BasicBlock, IFillable, IPlantable
{
    private readonly Boolean hasNeutralTint;
    private readonly TextureLayout wet;

    private Sides<Int32> wetTextureIndices = null!;

    /// <summary>
    ///     Create a new <see cref="DirtBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The named ID of the block.</param>
    /// <param name="normal">The normal texture layout.</param>
    /// <param name="wet">The texture layout when wet.</param>
    /// <param name="hasNeutralTint">Whether the block has a neutral tint.</param>
    /// <param name="supportsFullGrowth">Whether the block supports full growth.</param>
    protected CoveredDirtBlock(String name, String namedID, TextureLayout normal, TextureLayout wet,
        Boolean hasNeutralTint, Boolean supportsFullGrowth) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            normal)
    {
        this.hasNeutralTint = hasNeutralTint;
        SupportsFullGrowth = supportsFullGrowth;

        this.wet = wet;
    }

    /// <inheritdoc />
    public virtual Boolean IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.Viscosity < 100;
    }

    /// <inheritdoc />
    public Boolean SupportsFullGrowth { get; }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        base.OnSetup(indexProvider, visuals);

        wetTextureIndices = wet.GetTextureIndices(indexProvider);
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        ISimple.MeshData mesh = base.GetMeshData(info);

        mesh = mesh with {Tint = hasNeutralTint ? TintColor.Neutral : TintColor.None};

        if (info.Fluid.IsFluid) mesh = mesh with {TextureIndex = wetTextureIndices[info.Side]};

        return mesh;
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return DirtBehaviour.CanPlaceCovered(world, position, actor);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        DirtBehaviour.DoPlaceCovered(this, world, position, actor);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, BlockSide side)
    {
        DirtBehaviour.BlockUpdateCovered(world, position, side);
    }
}
