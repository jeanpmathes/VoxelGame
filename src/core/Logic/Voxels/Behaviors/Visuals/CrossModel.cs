// <copyright file="CrossModel.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     For <see cref="Complex" /> blocks which use the predefined cross mesh.
/// </summary>
public partial class CrossModel : BlockBehavior, IBehavior<CrossModel, BlockBehavior, Block>
{
    private readonly SingleTextured texture;

    [Constructible]
    private CrossModel(Block subject) : base(subject)
    {
        texture = subject.Require<SingleTextured>();

        subject.BoundingVolume.ContributeConstant(BoundingVolume.CrossBlock(height: 1.0, width: 0.71));
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
    }

    private Mesh GetMesh(Mesh original, MeshContext context)
    {
        Int32 textureIndex = texture.GetTextureIndex(context.State, context.TextureIndexProvider, isBlock: true);

        return Meshes.CreateCrossMesh(textureIndex);
    }
}
