// <copyright file="AttachedBlockDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Places flat blocks at walls.
/// </summary>
/// <typeparam name="TFilter">The block behavior type to filter on.</typeparam>
public class AttachedBlockDecoration<TFilter> : Decoration where TFilter : BlockBehavior, IBehavior<TFilter, BlockBehavior, Block>
{
    private readonly Block block;

    /// <summary>
    ///     Creates a new instance of the <see cref="AttachedBlockDecoration{TFilter}" /> class.
    /// </summary>
    /// <param name="name">The name of the decoration. </param>
    /// <param name="block">The block to place.</param>
    public AttachedBlockDecoration(String name, Block block) : base(name, new WallDecorator())
    {
        this.block = block;
    }

    /// <inheritdoc />
    public override Int32 Size => 1;

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        foreach (Orientation orientation in Orientations.All)
        {
            Content? neighbor = grid.GetContent(position.Offset(orientation));

            if (neighbor is not {Block: {IsFullySolid: true} neighborBlock}) continue;
            if (!neighborBlock.Block.Is<TFilter>()) continue;

            grid.SetContent(new Content(block.States.GenerationDefault.WithAttachment(orientation.ToSide()), FluidInstance.Default), position);

            break;
        }
    }
}
