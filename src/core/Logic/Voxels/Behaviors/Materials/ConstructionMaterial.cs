// <copyright file="ConstructionMaterial.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Blocks used for basic construction.
/// </summary>
public partial class ConstructionMaterial : BlockBehavior, IBehavior<ConstructionMaterial, BlockBehavior, Block>
{
    [Constructible]
    private ConstructionMaterial(Block subject) : base(subject)
    {
        subject.Require<Connectable>().Strength.Initializer.ContributeConstant(Connectable.Strengths.All);
    }
}
