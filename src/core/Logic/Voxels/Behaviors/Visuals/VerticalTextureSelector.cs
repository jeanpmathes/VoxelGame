// <copyright file="VerticalTextureSelector.cs" company="VoxelGame">
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
