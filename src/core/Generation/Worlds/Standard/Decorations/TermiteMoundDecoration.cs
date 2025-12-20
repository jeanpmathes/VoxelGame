// <copyright file="TermiteMoundDecoration.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     A termite mound.
/// </summary>
public class TermiteMoundDecoration : Decoration
{
    private readonly Shape3D shape;

    /// <summary>
    ///     Creates a new instance of the <see cref="TermiteMoundDecoration" /> class.
    /// </summary>
    public TermiteMoundDecoration(String name, Decorator decorator) : base(name, decorator)
    {
        const Int32 diameter = 5;

        shape = new Spheroid {Radius = (1.5, diameter / 2.0, 1.5)};
        Size = diameter;
    }

    /// <inheritdoc />
    public override Int32 Size { get; }

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        Vector3i extents = new(Size / 2);
        Vector3i center = position - extents;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
            CheckPosition(grid, center, (x, y, z), extents);
    }

    private void CheckPosition(IGrid grid, Vector3i center, Vector3i offset, Vector3i extents)
    {
        Vector3i current = center + offset;

        if (!shape.Contains(offset - extents)) return;

        Content? content = grid.GetContent(current);

        if (content is not {Block: var block})
            return;

        if (block.IsReplaceable || block.Block.Is<Regolith>())
            grid.SetContent(Content.CreateGenerated(Blocks.Instance.Organic.TermiteMound), current);
    }
}
