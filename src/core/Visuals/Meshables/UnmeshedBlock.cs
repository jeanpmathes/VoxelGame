// <copyright file="UnmeshedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
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
    public UnmeshedBlock(UInt32 blockID, CID contentID, String name) : base(blockID, contentID, name)
    {
        Require<Unmeshed>();
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Unmeshed;

    /// <param name="validator"></param>
    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Is<Meshed>()) 
            validator.ReportWarning("Unmeshed block should not have the Meshed behavior");
    }

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    public override ColorS GetDominantColor(State state, ColorS positionTint)
    {
        return ColorS.Black;
    }
}
