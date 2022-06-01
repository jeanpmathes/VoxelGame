// <copyright file="InsetDirtBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A dirt-like block that is a bit lower then normal dirt.
///     Data bit usage: <c>------</c>.
/// </summary>
public class InsetDirtBlock : Block, IVaryingHeight, IFillable, IPlantable, IPotentiallySolid, IAshCoverable
{
    private static readonly int height = IHeightVariable.MaximumHeight - 1;

    private readonly TextureLayout dryLayout;

    private readonly BoundingVolume volume;
    private readonly TextureLayout wetLayout;

    private int[] dryTextureIndices = null!;
    private int[] wetTextureIndices = null!;

    internal InsetDirtBlock(string name, string namedId, TextureLayout dry, TextureLayout wet,
        bool supportsFullGrowth) :
        base(
            name,
            namedId,
            BlockFlags.Solid,
            BoundingVolume.Block)
    {
        dryLayout = dry;
        wetLayout = wet;

        SupportsFullGrowth = supportsFullGrowth;

        volume = BoundingVolume.BlockWithHeight(height);
    }

    /// <inheritdoc />
    public void CoverWithAsh(World world, Vector3i position)
    {
        world.SetBlock(GrassBurned.AsInstance(), position);
    }

    /// <inheritdoc />
    public bool SupportsFullGrowth { get; }

    /// <inheritdoc />
    public void BecomeSolid(World world, Vector3i position)
    {
        world.SetBlock(Dirt.AsInstance(), position);
    }

    /// <inheritdoc />
    public int GetHeight(uint data)
    {
        return height;
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        int texture = info.Fluid.IsFluid
            ? wetTextureIndices[(int) info.Side]
            : dryTextureIndices[(int) info.Side];

        return new IVaryingHeight.MeshData
        {
            TextureIndex = texture,
            Tint = TintColor.None
        };
    }

    /// <inheritdoc />
    protected override void Setup(ITextureIndexProvider indexProvider)
    {
        dryTextureIndices = dryLayout.GetTexIndexArray();
        wetTextureIndices = wetLayout.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volume;
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
