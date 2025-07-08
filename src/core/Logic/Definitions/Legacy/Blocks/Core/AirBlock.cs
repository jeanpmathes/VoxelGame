// <copyright file="AirBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     AirBlocks are blocks that have no collision and are not rendered. They are used for the air block that stands for
///     the absence of other blocks.
///     Data bit usage: <c>------</c>
/// </summary>
public class AirBlock : Block, IFillable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AirBlock" /> class.
    /// </summary>
    /// <param name="name">The name of this block</param>
    /// <param name="namedID">The unique and unlocalized name of this block.</param>
    internal AirBlock(String name, String namedID) :
        base(
            name,
            namedID,
            BlockFlags.Empty,
            BoundingVolume.Block) {}

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, Actor? actor)
    {
        return false;
    }

    /// <inheritdoc />
    protected override Boolean CanDestroy(World world, Vector3i position, UInt32 data, Actor? actor)
    {
        return false;
    }
}
