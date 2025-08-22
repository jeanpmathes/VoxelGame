﻿// <copyright file="ConstantHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Height;

/// <summary>
/// Defines the partial block height of a block as a constant value.
/// </summary>
/// <seealso cref="PartialHeight"/>
public class ConstantHeight : BlockBehavior, IBehavior<ConstantHeight, BlockBehavior, Block>
{
    private ConstantHeight(Block subject) : base(subject)
    {
        HeightInitializer = Aspect<Int32, Block>.New<Exclusive<Int32, Block>>(nameof(HeightInitializer), this);
        
        subject.Require<PartialHeight>().Height.ContributeFunction((_, _) => Height, exclusive: true);
    }
    
    /// <inheritdoc/>
    public static ConstantHeight Construct(Block input)
    {
        return new ConstantHeight(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        Height = HeightInitializer.GetValue(original: PartialHeight.MaximumHeight, Subject);
    }

    /// <inheritdoc/>
    protected override void OnValidate(IResourceContext context)
    {
        if (Height < PartialHeight.MinimumHeight)
        {
            context.ReportWarning(this, "Constant partial height value is below the minimum valid value");

            Height = PartialHeight.MinimumHeight;
        }
        
        if (Height > PartialHeight.MaximumHeight)
        {
            context.ReportWarning(this, "Constant partial height value is above the maximum valid value");

            Height = PartialHeight.MaximumHeight;
        }
    }

    /// <summary>
    /// The constant height of the block.
    /// </summary>
    public Int32 Height { get; private set; } = PartialHeight.MaximumHeight;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Height"/> property.
    /// </summary>
    public Aspect<Int32, Block> HeightInitializer { get; }
}
