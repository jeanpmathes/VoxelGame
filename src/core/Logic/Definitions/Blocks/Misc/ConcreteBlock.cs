// <copyright file="ConcreteBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
public class ConcreteBlock : Block, IVaryingHeight, IWideConnectable, IThinConnectable, IOverlayTextureProvider
{
    private readonly TextureLayout layout;

    private readonly List<BoundingVolume> volumes = new();
    private int[] textures = null!;

    internal ConcreteBlock(string name, string namedId, TextureLayout layout) :
        base(
            name,
            namedId,
            BlockFlags.Functional with {IsOpaque = true},
            BoundingVolume.Block)
    {
        this.layout = layout;

        static BoundingVolume CreateVolume(uint data)
        {
            Decode(data, out _, out int height);

            return BoundingVolume.BlockWithHeight(height);
        }

        for (uint data = 0; data <= 0b11_1111; data++) volumes.Add(CreateVolume(data));
    }

    /// <inheritdoc />
    public int TextureIdentifier => layout.Bottom;

    /// <inheritdoc />
    public TintColor GetTintColor(Content content)
    {
        Decode(content.Block.Data, out BlockColor color, out _);

        return color.ToTintColor();
    }

    /// <inheritdoc />
    public int GetHeight(uint data)
    {
        Decode(data, out _, out int height);

        return height;
    }

    IVaryingHeight.MeshData IVaryingHeight.GetMeshData(BlockMeshInfo info)
    {
        Decode(info.Data, out BlockColor color, out _);

        return new IVaryingHeight.MeshData
        {
            TextureIndex = textures[(int) info.Side],
            Tint = color.ToTintColor()
        };
    }

    /// <inheritdoc />
    public bool IsConnectable(World world, BlockSide side, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;

        return GetHeight(block.Data) == IHeightVariable.MaximumHeight;
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        textures = layout.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b11_1111];
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
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
        if (base.Place(world, position))
            world.SetBlock(this.AsInstance(Encode(BlockColor.Default, level.GetBlockHeight())), position);
    }

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        Decode(data, out BlockColor color, out int height);
        var next = (BlockColor) ((int) color + 1);
        entity.World.SetBlock(this.AsInstance(Encode(next, height)), position);
    }

    private static uint Encode(BlockColor color, int height)
    {
        var val = 0;
        val |= ((int) color << 3) & 0b11_1000;
        val |= (height / 2) & 0b00_0111;

        return (uint) val;
    }

    private static void Decode(uint data, out BlockColor color, out int height)
    {
        color = (BlockColor) ((data & 0b11_1000) >> 3);
        height = (int) (data & 0b00_0111) * 2 + 1;
    }
}


