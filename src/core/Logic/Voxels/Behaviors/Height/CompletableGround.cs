// <copyright file="CompletableGround.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     A <see cref="PartialHeight" /> block which can be completed to serve as full ground for another block.
///     Other blocks can then for example use this when being placed and expecting full ground to be present.
/// </summary>
public class CompletableGround : BlockBehavior, IBehavior<CompletableGround, BlockBehavior, Block>
{
    private Block replacement = null!;

    private CompletableGround(Block subject) : base(subject)
    {
        ReplacementInitializer = Aspect<String?, Block>.New<Exclusive<String?, Block>>(nameof(ReplacementInitializer), this);
    }

    /// <summary>
    ///     The block that will replace this block to complete it.
    /// </summary>
    public String? Replacement { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="Replacement" /> property.
    /// </summary>
    public Aspect<String?, Block> ReplacementInitializer { get; }

    /// <inheritdoc />
    public static CompletableGround Construct(Block input)
    {
        return new CompletableGround(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Replacement = ReplacementInitializer.GetValue(original: null, Subject);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Replacement == null)
            validator.ReportWarning("Replacement block is not set");

        if (Replacement == Subject.NamedID)
            validator.ReportWarning("Replacement block cannot be the same as the block itself");

        replacement = Blocks.Instance.SafelyTranslateNamedID(Replacement);

        if (replacement == Blocks.Instance.Core.Error && Replacement != Blocks.Instance.Core.Error.NamedID)
            validator.ReportWarning($"The replacement block '{Replacement}' could not be found");

        if (!replacement.IsFullySolid(replacement.States.Default))
            validator.ReportWarning("Replacement block is not fully solid");
    }

    /// <summary>
    ///     Make this block into a complete, solid block.
    /// </summary>
    /// <param name="world">The world in which the operation takes place.</param>
    /// <param name="position">The position of the block.</param>
    public void BecomeComplete(World world, Vector3i position)
    {
        if (replacement == Blocks.Instance.Core.Error)
            return;

        world.SetBlock(replacement.States.Default, position);
    }
}
