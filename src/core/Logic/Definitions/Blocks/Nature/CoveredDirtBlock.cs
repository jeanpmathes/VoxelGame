// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
    private readonly bool hasNeutralTint;
    private readonly TextureLayout wet;

    private int[] wetTextureIndices = null!;

    /// <summary>
    ///     Create a new <see cref="DirtBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedId">The named ID of the block.</param>
    /// <param name="normal">The normal texture layout.</param>
    /// <param name="wet">The texture layout when wet.</param>
    /// <param name="hasNeutralTint">Whether the block has a neutral tint.</param>
    /// <param name="supportsFullGrowth">Whether the block supports full growth.</param>
    protected CoveredDirtBlock(string name, string namedId, TextureLayout normal, TextureLayout wet,
        bool hasNeutralTint, bool supportsFullGrowth) :
        base(
            name,
            namedId,
            BlockFlags.Basic,
            normal)
    {
        this.hasNeutralTint = hasNeutralTint;
        SupportsFullGrowth = supportsFullGrowth;

        this.wet = wet;
    }

    /// <inheritdoc />
    public virtual bool AllowInflow(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        return fluid.Viscosity < 100;
    }

    /// <inheritdoc />
    public bool SupportsFullGrowth { get; }

    /// <inheritdoc />
    protected override void Setup(ITextureIndexProvider indexProvider)
    {
        base.Setup(indexProvider);

        wetTextureIndices = wet.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        ISimple.MeshData mesh = base.GetMeshData(info);

        mesh = mesh with {Tint = hasNeutralTint ? TintColor.Neutral : TintColor.None};

        if (info.Fluid.IsFluid) mesh = mesh with {TextureIndex = wetTextureIndices[(int) info.Side]};

        return mesh;
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return DirtBehaviour.CanPlaceCovered(world, position, entity);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        DirtBehaviour.DoPlaceCovered(this, world, position, entity);
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        DirtBehaviour.BlockUpdateCovered(world, position, side);
    }
}

