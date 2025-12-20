// <copyright file="Crops.cs" company="VoxelGame">
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

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     Crops grow on farmland and can be harvested for food or other resources.
/// </summary>
public class Crops(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Flax { get; } = builder.BuildDenseCrop(new CID(nameof(Flax)), Language.Flax);

    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Potato { get; } = builder.BuildDenseCrop(new CID(nameof(Potato)), Language.Potato);

    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Onion { get; } = builder.BuildDenseCrop(new CID(nameof(Onion)), Language.Onion);

    /// <summary>
    ///     Wheat is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Wheat { get; } = builder.BuildDenseCrop(new CID(nameof(Wheat)), Language.Wheat);

    /// <summary>
    ///     Maize is a crop plant that grows on farmland.
    ///     Maize grows two blocks high. It requires water to fully grow.
    /// </summary>
    public Crop Maize { get; } = builder.BuildDoubleCrop(new CID(nameof(Maize)), Language.Maize);

    /// <summary>
    ///     The pumpkin plant grows pumpkin fruits.
    /// </summary>
    public Crop Pumpkin { get; } = builder.BuildFruitCrop(new CID(nameof(Pumpkin)), (Language.PumpkinPlant, Language.Pumpkin));

    /// <summary>
    ///     The melon plant grows melon fruits.
    /// </summary>
    public Crop Melon { get; } = builder.BuildFruitCrop(new CID(nameof(Melon)), (Language.MelonPlant, Language.Melon));
}
