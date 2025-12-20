// <copyright file="UnmeshedBlock.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Blocks which do not use any meshing at all and thus do not contribute any mesh data.
/// </summary>
public class UnmeshedBlock : Block
{
    /// <inheritdoc />
    public UnmeshedBlock(UInt32 blockID, CID contentID, String name) : base(blockID, contentID, name)
    {
        Require<Unmeshed>();
    }

    /// <inheritdoc />
    public override Meshable Meshable => Meshable.Unmeshed;

    /// <param name="validator"></param>
    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Is<Meshed>())
            validator.ReportWarning("Unmeshed block should not have the Meshed behavior");
    }

    /// <inheritdoc />
    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    public override void Mesh(Vector3i position, State state, MeshingContext context)
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    public override ColorS GetDominantColor(State state, ColorS positionTint)
    {
        return ColorS.Black;
    }
}
