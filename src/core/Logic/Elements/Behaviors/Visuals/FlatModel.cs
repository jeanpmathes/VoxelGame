// <copyright file="FlatModel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
///     For <see cref="Complex" /> blocks which use the predefined flat mesh.
/// </summary>
public class FlatModel : BlockBehavior, IBehavior<FlatModel, BlockBehavior, Block>
{
    private readonly Sided siding;
    private readonly SingleTextured texture;

    private FlatModel(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();
        texture = subject.Require<SingleTextured>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);

        WidthInitializer = Aspect<Double, Block>.New<Exclusive<Double, Block>>(nameof(WidthInitializer), this);
    }

    /// <summary>
    ///     The width of the flat model, mainly used for collision.
    /// </summary>
    public Double Width { get; private set; } = 1.0;

    /// <summary>
    ///     Aspect used to initialize the <see cref="Width" /> property.
    /// </summary>
    public Aspect<Double, Block> WidthInitializer { get; }

    /// <inheritdoc />
    public static FlatModel Construct(Block input)
    {
        return new FlatModel(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Width = WidthInitializer.GetValue(original: 1.0, Subject);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Sides sides = siding.GetSides(state);

        return BoundingVolume.FlatBlock(sides, Width, depth: 0.1);
    }

    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider _, VisualConfiguration _) = context; // todo: create struct for this tuple

        Sides sides = siding.GetSides(state);
        Int32 textureIndex = texture.GetTextureIndex(state, textureIndexProvider, isBlock: true);

        return BlockMeshes.CreateFlatModel(
            sides,
            offset: 0.01f,
            textureIndex);
    }
}
