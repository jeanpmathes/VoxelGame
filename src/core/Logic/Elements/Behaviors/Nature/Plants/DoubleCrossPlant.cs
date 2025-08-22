// <copyright file="DoubleCrossPlant.cs" company="VoxelGame">
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
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;

/// <summary>
/// A <see cref="Plant"/> that uses the <see cref="Foliage.LayoutType.Cross"/> layout and consists of two parts.
/// </summary>
public class DoubleCrossPlant : BlockBehavior, IBehavior<DoubleCrossPlant, BlockBehavior, Block>
{
    private readonly Composite composite;
    
    private DoubleCrossPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();
        
        composite = subject.Require<Composite>();
        composite.MaximumSizeInitializer.ContributeConstant((1, 2, 1));
        
        var foliage = subject.Require<Foliage>();
        foliage.LayoutInitializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);
        foliage.Part.ContributeFunction(GetPart);
        
        subject.BoundingVolume.ContributeFunction((_, _) => BoundingVolume.CrossBlock(height: 1.0, width: Width));
        
        WidthInitializer = Aspect<Double, Block>.New<Exclusive<Double, Block>>(nameof(WidthInitializer), this);
    }
    
    /// <summary>
    /// The width of the plant, used for the bounding volume.
    /// </summary>
    public Double Width { get; private set; } = 0.71;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Width"/> property.
    /// </summary>
    public Aspect<Double, Block> WidthInitializer { get; }

    /// <inheritdoc />
    public static DoubleCrossPlant Construct(Block input)
    {
        return new DoubleCrossPlant(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Width = WidthInitializer.GetValue(original: 0.71, Subject);
    }

    private Foliage.PartType GetPart(Foliage.PartType original, State state)
    {
        return composite.GetPartPosition(state).Y == 0 ? Foliage.PartType.DoubleLower : Foliage.PartType.DoubleUpper;
    }
}
