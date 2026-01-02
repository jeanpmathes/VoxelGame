// <copyright file="CompositeModelled.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Performs part selection for a <see cref="Modelled" /> block based on the <see cref="Composite" /> parts.
/// </summary>
public partial class CompositeModelled : BlockBehavior, IBehavior<CompositeModelled, BlockBehavior, Block>
{
    private readonly Composite composite;

    [Constructible]
    private CompositeModelled(Block subject) : base(subject)
    {
        composite = subject.Require<Composite>();
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
    }

    private Selector GetSelector(Selector original, State state)
    {
        Vector3i part = composite.GetPartPosition(state);

        // Maybe the stored models are not correct?
        part.X = composite.GetSize(state).X - part.X - 1;
        part.Z = composite.GetSize(state).Z - part.Z - 1;

        return original.WithPart(part);
    }
}
