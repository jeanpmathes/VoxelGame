// <copyright file="FlatBlockDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
///     Places flat blocks at walls.
/// </summary>
public class FlatBlockDecoration : Decoration // todo: rename to attached block decoration or so
{
    private readonly Block block;
    private readonly ISet<Block> filter;

    /// <summary>
    ///     Creates a new instance of the <see cref="FlatBlockDecoration" /> class.
    /// </summary>
    /// <param name="name">The name of the decoration. </param>
    /// <param name="block">The block to place.</param>
    /// <param name="filter">The blocks to place on.</param>
    public FlatBlockDecoration(String name, Block block, ISet<Block> filter) : base(name, new WallDecorator())
    {
        this.block = block;
        this.filter = filter;
    }

    /// <inheritdoc />
    public override Int32 Size => 1;

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, in PlacementContext placementContext, IGrid grid)
    {
        foreach (Orientation orientation in Orientations.All)
        {
            Content? neighbor = grid.GetContent(orientation.Offset(position));

            if (neighbor is not {Block: {IsFullySolid: true} neighborBlock}) continue;
            if (!filter.Contains(neighborBlock.Block)) continue;
            
            // todo: think of a way to get orientation.Opposite() to the block, similar issue as in the cover placement with snow
            // todo: also probably start of with the GenerationState instead of States.Default

            grid.SetContent(new Content(block.States.Default, FluidInstance.Default), position);

            break;
        }
    }
}
