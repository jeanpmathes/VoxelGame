// <copyright file="ThinConnectingBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A thin block that connects to other blocks.
///     Data bit usage: <c>--nesw</c>
/// </summary>
// n: connected north
// e: connected east
// s: connected south
// w: connected west
public class ThinConnectingBlock : ConnectingBlock<IThinConnectable>, IThinConnectable, IComplex
{
    private readonly RID postModel;
    private readonly RID sideModel;
    private readonly RID extensionModel;

    private readonly List<BlockMesh> meshes = new(capacity: 16);
    private readonly List<BoundingVolume> volumes = [];

    /// <inheritdoc />
    internal ThinConnectingBlock(
        String name,
        String namedID,
        Boolean isOpaque,
        RID postModel,
        RID sideModel,
        RID extensionModel) :
        base(
            name,
            namedID,
            new BlockFlags
            {
                IsSolid = true,
                IsOpaque = isOpaque
            },
            new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.0625f)))
    {
        this.postModel = postModel;
        this.sideModel = sideModel;
        this.extensionModel = extensionModel;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        BlockMesh mesh = meshes[(Int32) info.Data & 0b00_1111];

        return mesh.GetMeshData();
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        BlockModel post = modelProvider.GetModel(postModel);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) sides =
            modelProvider.GetModel(sideModel).CreateAllOrientations(rotateTopAndBottomTexture: false);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions =
            modelProvider.GetModel(extensionModel).CreateAllOrientations(rotateTopAndBottomTexture: false);

        for (UInt32 data = 0b00_0000; data <= 0b00_1111; data++)
        {
            BlockMesh mesh = BlockModel.GetCombinedMesh(textureIndexProvider,
                post,
                (data & 0b00_1000) == 0 ? sides.north : extensions.north,
                (data & 0b00_0100) == 0 ? sides.east : extensions.east,
                (data & 0b00_0010) == 0 ? sides.south : extensions.south,
                (data & 0b00_0001) == 0 ? sides.west : extensions.west);

            meshes.Add(mesh);

            volumes.Add(CreateVolume(data));
        }
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        List<BoundingVolume> connectors = new(BitHelper.CountSetBits(data));

        if ((data & 0b00_1000) != 0)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.21875f),
                    new Vector3d(x: 0.0625f, y: 0.5f, z: 0.21875f)));

        if ((data & 0b00_0100) != 0)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.78125f, y: 0.5f, z: 0.5f),
                    new Vector3d(x: 0.21875f, y: 0.5f, z: 0.0625f)));

        if ((data & 0b00_0010) != 0)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.78125f),
                    new Vector3d(x: 0.0625f, y: 0.5f, z: 0.21875f)));

        if ((data & 0b00_0001) != 0)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.21875f, y: 0.5f, z: 0.5f),
                    new Vector3d(x: 0.21875f, y: 0.5f, z: 0.0625f)));

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.0625f, y: 0.5f, z: 0.0625f),
            connectors.ToArray());
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_1111];
    }
}
