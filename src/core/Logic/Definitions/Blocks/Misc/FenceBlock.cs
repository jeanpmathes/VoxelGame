// <copyright file="FenceBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     This class represents a block which connects to blocks with the <see cref="IWideConnectable" /> interface. The
///     texture and indices of the BlockModels are ignored.
///     Data bit usage: <c>--nesw</c>
/// </summary>
// n: connected north
// e: connected east
// s: connected south
// w: connected west
public class FenceBlock : WideConnectingBlock, ICombustible
{
    private readonly List<BoundingVolume> volumes = [];

    /// <summary>
    ///     Create a new <see cref="FenceBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The named ID of the block.</param>
    /// <param name="texture">The texture to apply to the model.</param>
    /// <param name="postModel">The name of the post model. All model textures are ignored.</param>
    /// <param name="extensionModel">The name of the extension model. All model textures are ignored.</param>
    internal FenceBlock(String name, String namedID, String texture, RID postModel, RID extensionModel) :
        base(
            name,
            namedID,
            texture,
            isOpaque: true,
            postModel,
            extensionModel,
            new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.1875f, y: 0.5f, z: 0.1875f)))
    {
        for (UInt32 data = 0; data <= 0b00_1111; data++) volumes.Add(CreateVolume(data));
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        Boolean north = (data & 0b00_1000) != 0;
        Boolean east = (data & 0b00_0100) != 0;
        Boolean south = (data & 0b00_0010) != 0;
        Boolean west = (data & 0b00_0001) != 0;

        Int32 extensions = BitHelper.CountSetBooleans(north, east, south, west);

        var children = new BoundingVolume[2 * extensions];
        extensions = 0;

        if (north)
        {
            children[0] = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.28125f, z: 0.15625f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f));

            children[1] = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.71875f, z: 0.15625f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f));

            extensions += 2;
        }

        if (east)
        {
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.84375f, y: 0.28125f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f));

            children[extensions + 1] = new BoundingVolume(
                new Vector3d(x: 0.84375f, y: 0.71875f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f));

            extensions += 2;
        }

        if (south)
        {
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.28125f, z: 0.84375f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f));

            children[extensions + 1] = new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.71875f, z: 0.84375f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.15625f));

            extensions += 2;
        }

        if (west)
        {
            children[extensions] = new BoundingVolume(
                new Vector3d(x: 0.15625f, y: 0.28125f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f));

            children[extensions + 1] = new BoundingVolume(
                new Vector3d(x: 0.15625f, y: 0.71875f, z: 0.5f),
                new Vector3d(x: 0.15625f, y: 0.15625f, z: 0.125f));
        }

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.1875f, y: 0.5f, z: 0.1875f),
            children);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_1111];
    }
}
