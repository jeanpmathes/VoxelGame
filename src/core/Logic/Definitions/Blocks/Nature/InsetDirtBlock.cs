﻿// <copyright file="InsetDirtBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

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

    internal InsetDirtBlock(string name, string namedID, TextureLayout dry, TextureLayout wet,
        bool supportsFullGrowth) :
        base(
            name,
            namedID,
            BlockFlags.Solid with {IsOpaque = true},
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
        world.SetBlock(Logic.Blocks.Instance.GrassBurned.AsInstance(), position);
    }

    /// <inheritdoc />
    public bool SupportsFullGrowth { get; }

    /// <inheritdoc />
    public void BecomeSolid(World world, Vector3i position)
    {
        world.SetBlock(Logic.Blocks.Instance.Dirt.AsInstance(), position);
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
    protected override void OnSetup(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
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
    public override bool CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return DirtBehaviour.CanPlaceCovered(world, position, actor);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        DirtBehaviour.DoPlaceCovered(this, world, position, actor);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        DirtBehaviour.BlockUpdateCovered(world, position, side);
    }
}
