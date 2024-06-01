// <copyright file="ConcreteBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that can have different heights and colors. The heights correspond to fluid heights.
///     Data bit usage: <c>ccchhh</c>
/// </summary>
// c: color
// h: height
public class ConcreteBlock : Block, IVaryingHeight, IWideConnectable, IThinConnectable
{
    private readonly TextureLayout layout;

    private readonly List<BoundingVolume> volumes = [];
    private Int32[] textures = null!;

    internal ConcreteBlock(String name, String namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Functional with {IsOpaque = true},
            BoundingVolume.Block)
    {
        this.layout = layout;

        static BoundingVolume CreateVolume(UInt32 data)
        {
            Decode(data, out _, out Int32 height);

            return BoundingVolume.BlockWithHeight(height);
        }

        for (UInt32 data = 0; data <= 0b11_1111; data++) volumes.Add(CreateVolume(data));
    }

    /// <inheritdoc />
    public Int32 GetHeight(UInt32 data)
    {
        Decode(data, out _, out Int32 height);

        return height;
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        Decode(info.Data, out BlockColor color, out _);

        return new IVaryingHeight.MeshData
        {
            TextureIndex = textures[(Int32) info.Side],
            Tint = color.ToTintColor()
        };
    }

    /// <inheritdoc />
    public Boolean IsConnectable(World world, BlockSide side, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;

        return GetHeight(block.Data) == IHeightVariable.MaximumHeight;
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        textures = layout.GetTextureIndexArray(indexProvider);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b11_1111];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance(Encode(BlockColor.Default, IHeightVariable.MaximumHeight)), position);
    }

    /// <summary>
    ///     Try to place a concrete block at the given position.
    ///     The block will only be actually placed if the placement conditions are met, e.g. the position is replaceable.
    /// </summary>
    /// <param name="world">The world in which the block will be placed.</param>
    /// <param name="level">The height of the block, given in fluid levels.</param>
    /// <param name="position">The position where the block will be placed.</param>
    public void Place(World world, FluidLevel level, Vector3i position)
    {
        if (Place(world, position))
            world.SetBlock(this.AsInstance(Encode(BlockColor.Default, level.GetBlockHeight())), position);
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        Decode(data, out BlockColor color, out Int32 height);
        var next = (BlockColor) ((Int32) color + 1);
        actor.World.SetBlock(this.AsInstance(Encode(next, height)), position);
    }

    private static UInt32 Encode(BlockColor color, Int32 height)
    {
        var val = 0;
        val |= ((Int32) color << 3) & 0b11_1000;
        val |= (height / 2) & 0b00_0111;

        return (UInt32) val;
    }

    private static void Decode(UInt32 data, out BlockColor color, out Int32 height)
    {
        color = (BlockColor) ((data & 0b11_1000) >> 3);
        height = (Int32) (data & 0b00_0111) * 2 + 1;
    }
}
