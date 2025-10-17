// <copyright file="ConstantHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Defines the partial block height of a block as a constant value.
/// </summary>
/// <seealso cref="PartialHeight" />
public class ConstantHeight : BlockBehavior, IBehavior<ConstantHeight, BlockBehavior, Block>
{
    private ConstantHeight(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, _) => Height.Get(), exclusive: true);
    }

    /// <summary>
    ///     The constant height of the block.
    /// </summary>
    public ResolvedProperty<Int32> Height { get; } = ResolvedProperty<Int32>.New<Exclusive<Int32, Void>>(nameof(Height), PartialHeight.MaximumHeight);

    /// <inheritdoc />
    public static ConstantHeight Construct(Block input)
    {
        return new ConstantHeight(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Height.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Height.Get() < PartialHeight.MinimumHeight)
        {
            validator.ReportWarning("Constant partial height value is below the minimum valid value");

            Height.Override(PartialHeight.MinimumHeight);
        }

        if (Height.Get() > PartialHeight.MaximumHeight)
        {
            validator.ReportWarning("Constant partial height value is above the maximum valid value");

            Height.Override(PartialHeight.MaximumHeight);
        }
    }
}
