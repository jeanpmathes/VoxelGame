// <copyright file="CrossPlant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;

/// <summary>
/// A <see cref="Plant"/> that uses the <see cref="Foliage.LayoutType.Cross"/> layout.
/// </summary>
public class CrossPlant : BlockBehavior, IBehavior<CrossPlant, BlockBehavior, Block>
{
    private CrossPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();
        
        subject.Require<Foliage>().LayoutInitializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);
        
        subject.BoundingVolume.ContributeFunction((_, _) => BoundingVolume.CrossBlock(Height, Width));
        
        HeightInitializer = Aspect<Double, Block>.New<Exclusive<Double, Block>>(nameof(HeightInitializer), this);
        WidthInitializer = Aspect<Double, Block>.New<Exclusive<Double, Block>>(nameof(WidthInitializer), this);
    }
    
    /// <summary>
    /// The height of the plant, used for the bounding volume.
    /// </summary>
    public Double Height { get; private set; } = 1.0;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Height"/> property.
    /// </summary>
    public Aspect<Double, Block> HeightInitializer { get; }
    
    /// <summary>
    /// The width of the plant, used for the bounding volume.
    /// </summary>
    public Double Width { get; private set; } = 0.71;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Width"/> property.
    /// </summary>
    public Aspect<Double, Block> WidthInitializer { get; }
    
    /// <inheritdoc />
    public static CrossPlant Construct(Block input)
    {
        return new CrossPlant(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Height = HeightInitializer.GetValue(original: 1.0, Subject);
        Width = WidthInitializer.GetValue(original: 0.71, Subject);
    }
}
