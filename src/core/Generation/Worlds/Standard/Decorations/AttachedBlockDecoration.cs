// <copyright file="AttachedBlockDecoration.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
