// <copyright file="Fabricated.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Materials;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     These blocks are fabricated from other materials and are mostly decorative.
/// </summary>
public class Fabricated(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Wool is a flammable material, that allows its color to be changed.
    /// </summary>
    public Block Wool { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Wool)), Language.Wool)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("wool")))
        .WithBehavior<Combustible>()
        .WithBehavior<Paintable>()
        .Complete();

    /// <summary>
    ///     Decorated wool is similar to wool, decorated with golden ornaments.
    /// </summary>
    public Block WoolDecorated { get; } = builder
        .BuildSimpleBlock(new CID(nameof(WoolDecorated)), Language.WoolDecorated)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("wool_decorated")))
        .WithBehavior<Combustible>()
        .WithBehavior<Paintable>()
        .Complete();

    /// <summary>
    ///     Carpets can be used to cover the floor. Their color can be changed.
    /// </summary>
    public Block Carpet { get; } = builder
        .BuildComplexBlock(new CID(nameof(Carpet)), Language.Carpet)
        .WithBehavior<Modelled>(modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("carpet")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Paintable>()
        .Complete();

    /// <summary>
    ///     Decorated carpets are similar to carpets, decorated with golden ornaments.
    /// </summary>
    public Block CarpetDecorated { get; } = builder
        .BuildComplexBlock(new CID(nameof(CarpetDecorated)), Language.CarpetDecorated)
        .WithBehavior<Modelled>(modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("carpet_decorated")]))
        .WithBoundingVolume(new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)))
        .WithBehavior<Fillable>()
        .WithBehavior<Paintable>()
        .Complete();
}
