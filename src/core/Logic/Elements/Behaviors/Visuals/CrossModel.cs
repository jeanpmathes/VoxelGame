// <copyright file="CrossModel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
///     For <see cref="Complex" /> blocks which use the predefined cross mesh.
/// </summary>
public class CrossModel : BlockBehavior, IBehavior<CrossModel, BlockBehavior, Block>
{
    private readonly SingleTextured texture;

    private CrossModel(Block subject) : base(subject)
    {
        texture = subject.Require<SingleTextured>();

        subject.BoundingVolume.ContributeConstant(BoundingVolume.CrossBlock(height: 1.0, width: 0.71));
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
    }

    /// <inheritdoc />
    public static CrossModel Construct(Block input)
    {
        return new CrossModel(input);
    }

    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) = context; // todo: create struct for this tuple

        Int32 textureIndex = texture.GetTextureIndex(state, textureIndexProvider, isBlock: true);

        return BlockMeshes.CreateCrossMesh(textureIndex);
    }
}
