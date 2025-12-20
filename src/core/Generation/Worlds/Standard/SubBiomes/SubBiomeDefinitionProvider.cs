// <copyright file="SubBiomeDefinitionProvider.cs" company="VoxelGame">
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

using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Generation.Worlds.Standard.Palettes;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

/// <summary>
///     Implementation of the <see cref="ISubBiomeDefinitionProvider" /> interface.
/// </summary>
#pragma warning disable CA1001 // SubBiomeDefinitionProvider is safe to not dispose.
#pragma warning disable S2931 // SubBiomeDefinition is safe to not dispose.
public class SubBiomeDefinitionProvider : ResourceProvider<SubBiomeDefinition>, ISubBiomeDefinitionProvider
{
    private SubBiomeDefinition? fallback;

    /// <inheritdoc />
    public SubBiomeDefinition GetSubBiomeDefinition(RID identifier)
    {
        return GetResource(identifier);
    }

    /// <inheritdoc />
    protected override void OnSetUp(IResourceContext context)
    {
        context.Require<Palette>(palette =>
        {
            CreateFallback(palette);

            return [];
        });
    }

    [MemberNotNull(nameof(fallback))]
    private void CreateFallback(Palette palette)
    {
        fallback = new SubBiomeDefinition("Fallback", palette)
        {
            Cover = new Cover.Nothing(),
            Layers =
            [
                Layer.CreateStone(width: 1),
                Layer.CreateStonyDampen(maxWidth: 98),
                Layer.CreateStone(width: 1)
            ]
        };
    }

    /// <inheritdoc />
    protected override SubBiomeDefinition CreateFallback()
    {
        if (fallback == null)
        {
            Context?.ReportWarning(this, "Fallback sub-biome definition creation failed, using alternative palette");

            CreateFallback(new Palette());
        }

        return fallback;
    }
}
