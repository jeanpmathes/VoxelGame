// <copyright file="GrowingBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that grows upwards and is destroyed if a certain ground block is not given.
///     Data bit usage: <c>---aaa</c>
/// </summary>
// a: age
public class GrowingBlock : BasicBlock, ICombustible
{
    private readonly Int32 maxHeight;
    private readonly String requiredGroundID;

    [SuppressMessage("Usage", "CA2213", Justification = IResource.ResourcesOwnedByContext)]
    private Block requiredGround = null!;

    internal GrowingBlock(String name, String namedID, TextureLayout layout, String ground, Int32 maxHeight) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            layout)
    {
        requiredGroundID = ground;
        this.maxHeight = maxHeight;
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        base.OnSetUp(textureIndexProvider, modelProvider, visuals);

        requiredGround = Elements.Blocks.Instance.SafelyTranslateNamedID(requiredGroundID);
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Block down = world.GetBlock(position.Below())?.Block ?? Elements.Blocks.Instance.Air;

        return down == requiredGround || down == this;
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        if (side == Side.Bottom)
        {
            Block below = world.GetBlock(position.Below())?.Block ?? Elements.Blocks.Instance.Air;

            if (below != requiredGround && below != this) ScheduleDestroy(world, position);
        }
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, UInt32 data)
    {
        var age = (Int32) (data & 0b00_0111);

        if (age < 7)
        {
            world.SetBlock(this.AsInstance((UInt32) (age + 1)), position);
        }
        else
        {
            if (!(world.GetBlock(position.Above())?.Block.IsReplaceable ?? false)) return;

            var height = 0;

            for (var offset = 0; offset < maxHeight; offset++)
                if (world.GetBlock(position.Below(offset))?.Block == this) height++;
                else break;

            if (height < maxHeight) Place(world, position.Above());
        }
    }
}
