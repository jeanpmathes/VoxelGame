// <copyright file="CrossPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Physics;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.Cross" /> layout.
/// </summary>
public partial class CrossPlant : BlockBehavior, IBehavior<CrossPlant, BlockBehavior, Block>
{
    [Constructible]
    private CrossPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();

        subject.Require<Foliage>().Layout.Initializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);

        subject.BoundingVolume.ContributeFunction((_, _) => BoundingVolume.CrossBlock(Height.Get(), Width.Get()));
    }

    /// <summary>
    ///     The height of the plant, used for the bounding volume.
    /// </summary>
    public ResolvedProperty<Double> Height { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Height), initial: 1.0);

    /// <summary>
    ///     The width of the plant, used for the bounding volume.
    /// </summary>
    public ResolvedProperty<Double> Width { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Width), initial: 0.71);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Height.Initialize(this);
        Width.Initialize(this);
    }
}
