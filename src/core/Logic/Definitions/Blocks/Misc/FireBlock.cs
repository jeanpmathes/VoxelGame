// <copyright file="FireBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     An animated block that attaches to sides.
///     Data bit usage: <c>-fblrt</c>
/// </summary>
// f: front
// b: back
// l: left
// r: right
// t: top
public class FireBlock : Block, IFillable, IComplex
{
    private const UInt32 UpdateOffset = 150;
    private const UInt32 UpdateVariation = 25;

    private readonly RID completeModelID;
    private readonly RID sideModelID;
    private readonly RID topModelID;

    private readonly List<BlockMesh> meshes = new(capacity: 32);

    private readonly List<BoundingVolume> volumes = [];

    internal FireBlock(String name, String namedID, RID completeModel, RID sideModel, RID topModel) :
        base(
            name,
            namedID,
            BlockFlags.Replaceable with {IsUnshaded = true},
            BoundingVolume.Block)
    {
        completeModelID = completeModel;
        sideModelID = sideModel;
        topModelID = topModel;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh mesh = meshes[(Int32) info.Data & 0b01_1111];

        return mesh.GetMeshData(isAnimated: true);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        BlockModel complete = modelProvider.GetModel(completeModelID);

        BlockModel side = modelProvider.GetModel(sideModelID);
        BlockModel top = modelProvider.GetModel(topModelID);

        PrepareMeshes(textureIndexProvider, complete, side, top);

        for (UInt32 data = 0; data <= 0b01_1111; data++) volumes.Add(CreateVolume(data));
    }

    private void PrepareMeshes(ITextureIndexProvider textureIndexProvider, BlockModel completeModel, BlockModel sideModel, BlockModel topModel)
    {
        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) =
            sideModel.CreateAllOrientations(rotateTopAndBottomTexture: true);

        for (UInt32 data = 0b00_0000; data <= 0b01_1111; data++)
            if (data == 0)
            {
                meshes.Add(completeModel.CreateMesh(textureIndexProvider));
            }
            else
            {
                List<BlockModel> requiredModels = new(capacity: 5);

                requiredModels.AddRange(Side.All.Sides()
                    .Where(side => side != Side.Bottom && IsFlagSet(data, side))
                    .Select(GetSideModel));

                BlockMesh combinedMesh = BlockModel.GetCombinedMesh(textureIndexProvider, requiredModels.ToArray());
                meshes.Add(combinedMesh);
            }

        BlockModel GetSideModel(Side side)
        {
            return side switch
            {
                Side.Front => south,
                Side.Back => north,
                Side.Left => west,
                Side.Right => east,
                Side.Top => topModel,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, message: null)
            };
        }
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        if (data == 0) return BoundingVolume.Block;

        Int32 count = BitHelper.CountSetBits(data);
        Debug.Assert(count > 0);

        BoundingVolume? parent = null;

        var children = new BoundingVolume[count - 1];
        var next = 0;

        foreach (Side side in Side.All.Sides())
        {
            if (side == Side.Bottom) continue;
            if (!IsFlagSet(data, side)) continue;

            Vector3d offset = side.Direction().ToVector3() * 0.4f;

            var child = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f) + offset,
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f) - offset.Absolute());

            if (parent == null)
                parent = child;
            else
                children[next++] = child;
        }

        if (parent == null)
            return BoundingVolume.Block;

        return next == 0
            ? parent
            : new BoundingVolume(parent.Center, parent.Extents, children);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b01_1111];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        if (world.HasFullAndSolidGround(position)) return true;

        return GetData(world, position) != 0;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance(world.HasFullAndSolidGround(position) ? 0 : GetData(world, position)), position);
        ScheduleUpdate(world, position, GetDelay(position));
    }

    private static UInt32 GetData(World world, Vector3i position)
    {
        UInt32 data = 0;

        foreach (Side side in Side.All.Sides())
        {
            if (side == Side.Bottom) continue;

            if (world.GetBlock(side.Offset(position))?.IsSolidAndFull ?? false) data |= GetFlag(side);
        }

        return data;
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid) Destroy(world, position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        if (side == Side.Bottom)
        {
            if (data != 0) return;

            data |= CreateSideData(world, position);

            SetData(data);
        }
        else
        {
            if (!IsFlagSet(data, side) || (world.GetBlock(side.Offset(position))?.IsSolidAndFull ?? false)) return;

            data ^= GetFlag(side);
            SetData(data);
        }

        void SetData(UInt32 dataToSet)
        {
            if (dataToSet != 0) world.SetBlock(this.AsInstance(dataToSet), position);
            else Destroy(world, position);
        }
    }

    private static UInt32 CreateSideData(World world, Vector3i position)
    {
        UInt32 data = 0;

        foreach (Side sideToCheck in Side.All.Sides())
        {
            if (sideToCheck == Side.Bottom) continue;

            if (world.GetBlock(sideToCheck.Offset(position))?.IsSolidAndFull ?? false) data |= GetFlag(sideToCheck);
        }

        return data;
    }

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, UInt32 data)
    {
        var canBurn = false;

        if (data == 0)
        {
            canBurn |= BurnAt(position.Below()); // Bottom.
            data = 0b01_1111;
        }

        canBurn = Side.All.Sides()
            .Where(side => side != Side.Bottom)
            .Where(side => IsFlagSet(data, side))
            .Aggregate(canBurn, (current, side) => current | BurnAt(side.Offset(position)));

        if (!canBurn) Destroy(world, position);

        ScheduleUpdate(world, position, GetDelay(position));

        Boolean BurnAt(Vector3i burnPosition)
        {
            if (world.GetBlock(burnPosition)?.Block is ICombustible block)
            {
                if (block.Burn(world, burnPosition, this))
                {
                    if (world.GetBlock(burnPosition.Below())?.Block is IAshCoverable coverable)
                        coverable.CoverWithAsh(world, burnPosition.Below());

                    Place(world, burnPosition);
                }

                return true;
            }

            return false;
        }
    }

    private static UInt32 GetDelay(Vector3i position)
    {
        return UpdateOffset +
               (BlockUtilities.GetPositionDependentNumber(position, UpdateVariation * 2) - UpdateVariation);
    }

    private static UInt32 GetFlag(Side side)
    {
        return side switch
        {
            Side.Front => 0b01_0000,
            Side.Back => 0b00_1000,
            Side.Left => 0b00_0100,
            Side.Right => 0b00_0010,
            Side.Top => 0b00_0001,
            _ => 0b00_0000
        };
    }

    private static Boolean IsFlagSet(UInt32 data, Side side)
    {
        return (data & GetFlag(side)) != 0;
    }
}
