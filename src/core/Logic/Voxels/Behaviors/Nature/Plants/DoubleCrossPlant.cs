// <copyright file="DoubleCrossPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.Cross" /> layout and consists of two parts.
/// </summary>
public partial class DoubleCrossPlant : BlockBehavior, IBehavior<DoubleCrossPlant, BlockBehavior, Block>
{
    private readonly Composite composite;

    [Constructible]
    private DoubleCrossPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();
        subject.Require<VerticalTextureSelector>();

        composite = subject.Require<Composite>();
        composite.MaximumSize.Initializer.ContributeConstant((1, 2, 1));

        var foliage = subject.Require<Foliage>();
        foliage.Layout.Initializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);
        foliage.Part.ContributeFunction(GetPart);

        subject.BoundingVolume.ContributeFunction((_, _) => BoundingVolume.CrossBlock(height: 1.0, Width.Get()));
    }

    /// <summary>
    ///     The width of the plant, used for the bounding volume.
    /// </summary>
    public ResolvedProperty<Double> Width { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Width), initial: 0.71);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Width.Initialize(this);
    }

    private Foliage.PartType GetPart(Foliage.PartType original, State state)
    {
        return composite.GetPartPosition(state).Y == 0 ? Foliage.PartType.DoubleLower : Foliage.PartType.DoubleUpper;
    }
}
