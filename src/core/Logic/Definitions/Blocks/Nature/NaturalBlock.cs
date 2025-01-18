// <copyright file="NaturalBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A natural block that can burn.
///     Data bit usage: <c>------</c>
/// </summary>
public class NaturalBlock : BasicBlock, ICombustible
{
    private readonly Boolean hasNeutralTint;

    /// <summary>
    ///     Creates a new instance of the <see cref="NaturalBlock" /> class.
    /// </summary>
    /// <param name="name">The internal block name.</param>
    /// <param name="namedID">The named id of the block.</param>
    /// <param name="hasNeutralTint">Whether the block has a neutral tint.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="layout">The texture layout.</param>
    public NaturalBlock(String name, String namedID, Boolean hasNeutralTint, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedID,
            flags,
            layout)
    {
        this.hasNeutralTint = hasNeutralTint;
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        ISimple.MeshData mesh = base.GetMeshData(info);

        if (hasNeutralTint) mesh = mesh with {Tint = ColorS.Neutral};

        return mesh;
    }
}
