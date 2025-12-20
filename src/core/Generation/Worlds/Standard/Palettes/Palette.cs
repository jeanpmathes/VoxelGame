// <copyright file="Palette.cs" company="VoxelGame">
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

using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Palettes;

/// <summary>
///     A palette of blocks and fluids to use for world generation.
/// </summary>
public sealed class Palette : IResource
{
    private readonly Content granite = Content.CreateGenerated(Blocks.Instance.Stones.Granite.Base);

    private readonly Content gravel = Content.CreateGenerated(Blocks.Instance.Environment.Gravel);
    private readonly Content gravelGroundwater = Content.CreateGenerated(Blocks.Instance.Environment.Gravel, Fluids.Instance.FreshWater);
    private readonly Content limestone = Content.CreateGenerated(Blocks.Instance.Stones.Limestone.Base);
    private readonly Content marble = Content.CreateGenerated(Blocks.Instance.Stones.Marble.Base);

    private readonly Content sand = Content.CreateGenerated(Blocks.Instance.Environment.Sand);
    private readonly Content sandGroundwater = Content.CreateGenerated(Blocks.Instance.Environment.Sand, Fluids.Instance.FreshWater);
    private readonly Content sandstone = Content.CreateGenerated(Blocks.Instance.Stones.Sandstone.Base);

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<Palette>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.GeneratorPalette;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    internal Content GetStone(Map.StoneType type)
    {
        return type switch
        {
            Map.StoneType.Sandstone => sandstone,
            Map.StoneType.Granite => granite,
            Map.StoneType.Limestone => limestone,
            Map.StoneType.Marble => marble,
            _ => throw Exceptions.UnsupportedEnumValue(type)
        };
    }

    internal Content GetLoose(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sand : gravel;
    }

    internal Content GetGroundwater(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sandGroundwater : gravelGroundwater;
    }
}
