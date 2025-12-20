// <copyright file="StructureDecoration.cs" company="VoxelGame">
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
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     A decoration that places structures.
/// </summary>
public class StructureDecoration : Decoration
{
    private readonly Structure structure;

    /// <summary>
    ///     Create a new structure decoration.
    /// </summary>
    /// <param name="name">The name of the decoration.</param>
    /// <param name="structure">The structure to place.</param>
    /// <param name="decorator">The decorator to use.</param>
    public StructureDecoration(String name, Structure structure, Decorator decorator) : base(name, decorator)
    {
        this.structure = structure;

        decorator.SetSizeHint(structure.Extents);

        Debug.Assert(Size <= Section.Size);
    }

    /// <inheritdoc />
    public sealed override Int32 Size => structure.Extents.MaxComponent();

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        Int32 xOffset = structure.Extents.X / 2;
        Int32 zOffset = structure.Extents.Z / 2;

        structure.Place(placementContext.Random.GetHashCode(), grid, position - new Vector3i(xOffset, y: 0, zOffset));
    }
}
