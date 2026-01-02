// <copyright file="DoubleCrossPlant.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     A <see cref="Plant" /> that uses the <see cref="Foliage.LayoutType.Cross" /> layout and consists of two parts.
/// </summary>
public partial class DoubleCrossPlant : BlockBehavior, IBehavior<DoubleCrossPlant, BlockBehavior, Block>
{
    private readonly Composite composite;

    [Constructible]
    private DoubleCrossPlant(Block subject) : base(subject)
    {
        subject.Require<Plant>();
        subject.Require<VerticalTextureSelector>();

        composite = subject.Require<Composite>();
        composite.MaximumSize.Initializer.ContributeConstant((1, 2, 1));

        var foliage = subject.Require<Foliage>();
        foliage.Layout.Initializer.ContributeConstant(Foliage.LayoutType.Cross, exclusive: true);
        foliage.Part.ContributeFunction(GetPart);

        subject.BoundingVolume.ContributeFunction((_, _) => BoundingVolume.CrossBlock(height: 1.0, Width.Get()));
    }

    /// <summary>
    ///     The width of the plant, used for the bounding volume.
    /// </summary>
    public ResolvedProperty<Double> Width { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(Width), initial: 0.71);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Width.Initialize(this);
    }

    private Foliage.PartType GetPart(Foliage.PartType original, State state)
    {
        return composite.GetPartPosition(state).Y == 0 ? Foliage.PartType.DoubleLower : Foliage.PartType.DoubleUpper;
    }
}
