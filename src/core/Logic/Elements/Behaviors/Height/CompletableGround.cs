// <copyright file="CompletableGround.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Height;

/// <summary>
/// A <see cref="PartialHeight"/> block which can be completed to serve as full ground for another block.
/// Other blocks can then for example use this when being placed and expecting full ground to be present.
/// </summary>
public class CompletableGround : BlockBehavior, IBehavior<CompletableGround, BlockBehavior, Block>
{
    private CompletableGround(Block subject) : base(subject)
    {
        ReplacementInitializer = Aspect<Block, Block>.New<Exclusive<Block, Block>>(nameof(ReplacementInitializer), this);
    }
    
    /// <inheritdoc/>
    public static CompletableGround Construct(Block input)
    {
        return new CompletableGround(input);
    }

    /// <summary>
    /// The block that will replace this block to complete it.
    /// </summary>
    public Block Replacement { get; private set; } = Blocks.Instance.Core.Error;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Replacement"/> property.
    /// </summary>
    public Aspect<Block, Block> ReplacementInitializer { get; }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        Replacement = ReplacementInitializer.GetValue(original: Blocks.Instance.Core.Error, Subject);
    }

    /// <inheritdoc/>
    protected override void OnValidate(IResourceContext context)
    {
        if (Replacement == Blocks.Instance.Core.Error)
            context.ReportWarning(this, "Replacement block is not set");

        if (!Replacement.IsFullySolid(Replacement.States.Default))
            context.ReportWarning(this, "Replacement block is not fully solid");
    }

    /// <summary>
    ///     Make this block into a complete, solid block.
    /// </summary>
    /// <param name="world">The world in which the operation takes place.</param>
    /// <param name="position">The position of the block.</param>
    public void BecomeComplete(World world, Vector3i position)
    {
        if (Replacement == Blocks.Instance.Core.Error)
            return;

        world.SetBlock(new BlockInstance(Replacement.States.Default), position);
    }
}
