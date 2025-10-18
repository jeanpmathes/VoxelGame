// <copyright file="FlatModel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     For <see cref="Complex" /> blocks which use the predefined flat mesh.
/// </summary>
public partial class FlatModel : BlockBehavior, IBehavior<FlatModel, BlockBehavior, Block>
{
    private readonly Sided siding;
    private readonly SingleTextured texture;

    [Constructible]
    private FlatModel(Block subject) : base(subject)
    {
        siding = subject.Require<Sided>();
        texture = subject.Require<SingleTextured>();

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
    }

    /// <summary>
    ///     The width of the flat model, mainly used for collision.
    /// </summary>
    public ResolvedProperty<Double> Width { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Width), initial: 1.0);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Width.Initialize(this);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Sides sides = siding.GetSides(state);

        return BoundingVolume.FlatBlock(sides, Width.Get(), depth: 0.1);
    }

    private Mesh GetMesh(Mesh original, MeshContext context)
    {
        State state = context.State;

        Sides sides = siding.GetSides(state);
        Int32 textureIndex = texture.GetTextureIndex(state, context.TextureIndexProvider, isBlock: true);

        return Meshes.CreateFlatMesh(
            sides,
            offset: 0.01f,
            textureIndex);
    }
}
