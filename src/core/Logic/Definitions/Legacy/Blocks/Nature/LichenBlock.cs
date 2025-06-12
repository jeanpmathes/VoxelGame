// <copyright file="LichenBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
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
///     The lichen block is a flat block that can stick to all sides of other blocks.
///     Data bit usage: <c>ssssss</c>
/// </summary>
// s: side
public class LichenBlock : Block, IFillable, IComplex
{
    private readonly TID texture;

    private readonly List<BlockMesh> meshes = new(capacity: 64);
    private readonly List<BoundingVolume> volumes = new(capacity: 64);

    internal LichenBlock(String name, String namedID, TID texture) :
        base(
            name,
            namedID,
            BlockFlags.Replaceable,
            BoundingVolume.Block)
    {
        this.texture = texture;

        for (UInt32 data = 0; data <= 0b11_1111; data++)
        {
            var sides = (Sides) data;

            volumes.Add(
                BoundingVolume.Combine(Side.All.Sides()
                    .Where(side => sides.HasFlag(side.ToFlag()))
                    .Select(side => BoundingVolume.FlatBlock(side, depth: 0.1))));
        }
    }

    /// <inheritdoc />
    public IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return meshes[(Int32) (info.Data & 0b11_1111)].GetMeshData();
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        Int32 textureIndex = textureIndexProvider.GetTextureIndex(texture);

        SideArray<BlockMesh> availableParts = new();

        foreach (Side side in Side.All.Sides())
            availableParts[side] = BlockMeshes.CreateFlatModel(
                side,
                offset: 0.01f,
                textureIndex);

        for (UInt32 data = 0; data <= 0b11_1111; data++)
        {
            List<BlockMesh> selectedParts = [];

            var sides = (Sides) data;

            foreach (Side side in Side.All.Sides())
            {
                if (!sides.HasFlag(side.ToFlag()))
                    continue;

                selectedParts.Add(availableParts[side]);
            }

            meshes.Add(BlockMesh.Combine(selectedParts));
        }
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) (data & 0b11_1111)];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Side side = actor?.TargetSide ?? Side.Front;

        return world.GetBlock(side.Opposite().Offset(position))?.IsSolidAndFull ?? false;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        var sides = Sides.None;

        foreach (Side side in Side.All.Sides())
            if (world.GetBlock(side.Offset(position))?.IsSolidAndFull ?? false)
                sides |= side.ToFlag();

        world.SetBlock(this.AsInstance((UInt32) sides), position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        var sides = (Sides) data;
        Sides oldSides = sides;

        if (world.GetBlock(side.Offset(position))?.IsSolidAndFull ?? false)
            sides |= side.ToFlag();
        else
            sides &= ~side.ToFlag();

        if (sides == oldSides)
            return;

        if (sides == Sides.None) Destroy(world, position);
        else world.SetBlock(this.AsInstance((UInt32) sides), position);
    }

    /// <inheritdoc />
    public override Content GeneratorUpdate(Content content)
    {
        return content.Block.Data == 0
            ? new Content(this.AsInstance((UInt32) Sides.Bottom), FluidInstance.Default)
            : content;
    }
}
