// <copyright file="StructureGeneratorDefinitionProvider.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.Structures;

/// <summary>
///     Implementation of <see cref="IStructureGeneratorDefinitionProvider" />.
/// </summary>
public class StructureGeneratorDefinitionProvider : ResourceProvider<StructureGeneratorDefinition>, IStructureGeneratorDefinitionProvider
{
    private static readonly StructureGeneratorDefinition fallback
        = new(
            "Fallback",
            StructureGeneratorDefinition.Kind.Surface,
            StaticStructure.CreateFallback(),
            Single.PositiveInfinity,
            Vector3i.Zero);

    /// <inheritdoc />
    public StructureGeneratorDefinition GetStructure(RID identifier)
    {
        return GetResource(identifier);
    }

    /// <inheritdoc />
    protected override StructureGeneratorDefinition CreateFallback()
    {
        return fallback;
    }
}
