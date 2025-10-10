// <copyright file="UnmeshedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks which do not use any meshing at all and thus do not contribute any mesh data.
/// </summary>
public class UnmeshedBlock : Block
{
    /// <inheritdoc />
    public UnmeshedBlock(UInt32 id, String namedID, String name) : base(id, namedID, name)
    {
        Require<Unmeshed>();
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Unmeshed;

    /// <inheritdoc />
    protected override void OnValidate()
    {
        if (Is<Meshed>()) Debug.Fail("cringe bro");
        // todo: use proper validation (through the resource context or whatever) here as meshed behavior does not make sense for unmeshed blocks
    }

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals)
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        // Intentionally left empty.
    }
}
