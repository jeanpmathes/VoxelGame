// <copyright file="CrossPlant.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Physics;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.Cross" /> layout.
/// </summary>
public partial class CrossPlant : BlockBehavior, IBehavior<CrossPlant, BlockBehavior, Block>
{
    [Constructible]
    private CrossPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();

        subject.Require<Foliage>().Layout.Initializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);

        subject.BoundingVolume.ContributeFunction((_, _) => BoundingVolume.CrossBlock(Height.Get(), Width.Get()));
    }

    /// <summary>
    ///     The height of the plant, used for the bounding volume.
    /// </summary>
    public ResolvedProperty<Double> Height { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Height), initial: 1.0);

    /// <summary>
    ///     The width of the plant, used for the bounding volume.
    /// </summary>
    public ResolvedProperty<Double> Width { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Width), initial: 0.71);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Height.Initialize(this);
        Width.Initialize(this);
    }
}
