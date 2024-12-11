// <copyright file="BasicBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     This class represents a simple block that is completely filled. <see cref="BasicBlock" />s themselves do not have
///     much function, but the class can be extended easily.
///     Data bit usage: <c>------</c>
/// </summary>
public class BasicBlock : Block, ISimple
{
    private readonly TextureLayout layout;
    private protected SideArray<Int32> sideTextureIndices = null!;

    /// <summary>
    ///     Create a new <see cref="BasicBlock" />.
    ///     A <see cref="BasicBlock" /> is a block that is completely filled and cannot be replaced.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The named ID.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="layout">The texture layout.</param>
    internal BasicBlock(String name, String namedID, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedID,
            flags with {IsFull = true, IsReplaceable = false},
            BoundingVolume.Block)
    {
        this.layout = layout;
    }

    /// <inheritdoc />
    ISimple.MeshData ISimple.GetMeshData(BlockMeshInfo info)
    {
        return GetMeshData(info);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        sideTextureIndices = layout.GetTextureIndices(indexProvider);
    }

    /// <summary>
    ///     Overwrite to defined different mesh data.
    /// </summary>
    protected virtual ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        return ISimple.CreateData(sideTextureIndices[info.Side], isTextureRotated: false);
    }
}
