// <copyright file="VerticalTextureSelector.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Selects the part of the single texture of a block based on its vertical position in a composite block.
/// </summary>
public partial class VerticalTextureSelector : BlockBehavior, IBehavior<VerticalTextureSelector, BlockBehavior, Block>
{
    private readonly Composite composite;

    [Constructible]
    private VerticalTextureSelector(Block subject) : base(subject)
    {
        composite = subject.Require<Composite>();

        subject.Require<SingleTextured>().ActiveTexture.ContributeFunction(GetActiveTexture);

        HorizontalOffset = Aspect<Int32, State>.New<Exclusive<Int32, State>>(nameof(HorizontalOffset), this);
    }

    /// <summary>
    ///     Provides an optional horizontal offset to adjust the texture selection.
    /// </summary>
    public Aspect<Int32, State> HorizontalOffset { get; }

    private TID GetActiveTexture(TID original, State state)
    {
        var x = (Byte) HorizontalOffset.GetValue(original: 0, state);
        var y = (Byte) composite.GetPartPosition(state).Y;

        return original.Offset(x, y);
    }
}
