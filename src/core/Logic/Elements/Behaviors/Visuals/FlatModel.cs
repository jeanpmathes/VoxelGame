// <copyright file="FlatModel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// For <see cref="Complex"/> blocks which use the predefined flat mesh.
/// </summary>
public class FlatModel : BlockBehavior, IBehavior<FlatModel, BlockBehavior, Block>
{
    private readonly SingleSided siding; // todo: make it Sided instead
    private readonly SingleTextured texture;
    
    private FlatModel(Block subject) : base(subject)
    {
        siding = subject.Require<SingleSided>();
        texture = subject.Require<SingleTextured>();
        
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
    }

    /// <inheritdoc />
    public static FlatModel Construct(Block input)
    {
        return new FlatModel(input);
    }

    // todo: verify that the stored sides and the sides in the world match - in the original code there was a lot of Opposite() used so check that the new code makes sense
    
    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Side side = siding.GetSide(state);
        return BoundingVolume.FlatBlock(side.ToOrientation(), width: 0.9, depth: 0.1);
    }
    
    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider _, VisualConfiguration _) = context; // todo: create struct for this tuple

        Side side = siding.GetSide(state);
        Int32 textureIndex = texture.GetTextureIndex(state, textureIndexProvider, isBlock: true);
        
        return BlockMeshes.CreateFlatModel(
            side.Opposite(),
            offset: 0.01f,
            textureIndex);
    }
}
