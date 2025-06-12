// <copyright file="InsetDirtBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A dirt-like block that is a bit lower than normal dirt.
///     Data bit usage: <c>------</c>.
/// </summary>
public class InsetDirtBlock : Block, IVaryingHeight, IFillable, IPlantable, IPotentiallySolid, IAshCoverable
{
    private static readonly Int32 height = IHeightVariable.MaximumHeight - 1;

    private readonly TextureLayout dryLayout;

    private readonly BoundingVolume volume;
    private readonly TextureLayout wetLayout;

    private SideArray<Int32> dryTextureIndices = null!;
    private SideArray<Int32> wetTextureIndices = null!;

    internal InsetDirtBlock(String name, String namedID, TextureLayout dry, TextureLayout wet,
        Boolean supportsFullGrowth) :
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
        world.SetBlock(Elements.Legacy.Blocks.Instance.GrassBurned.AsInstance(), position);
    }

    /// <inheritdoc />
    public Boolean SupportsFullGrowth { get; }

    /// <inheritdoc />
    public void BecomeSolid(World world, Vector3i position)
    {
        world.SetBlock(Elements.Legacy.Blocks.Instance.Dirt.AsInstance(), position);
    }

    /// <inheritdoc />
    public Int32 GetHeight(UInt32 data)
    {
        return height;
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        Int32 texture = info.Fluid.IsFluid
            ? wetTextureIndices[info.Side]
            : dryTextureIndices[info.Side];

        return new IVaryingHeight.MeshData
        {
            TextureIndex = texture,
            Tint = ColorS.None
        };
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        dryTextureIndices = dryLayout.GetTextureIndices(textureIndexProvider, isBlock: true);
        wetTextureIndices = wetLayout.GetTextureIndices(textureIndexProvider, isBlock: true);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volume;
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
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        DirtBehaviour.BlockUpdateCovered(world, position, side);
    }
}
