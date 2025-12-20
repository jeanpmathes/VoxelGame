// <copyright file="HideWorldOnTermination.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Hides all sections in the world when the world is terminated.
///     This prevents rendering of the no longer needed sections.
/// </summary>
public partial class HideWorldOnTermination : WorldComponent
{
    [Constructible]
    private HideWorldOnTermination(Core.Logic.World subject) : base(subject) {}

    /// <inheritdoc />
    public override void OnTerminate(Object? sender, EventArgs e)
    {
        foreach (Chunk chunk in Subject.Chunks.All)
            chunk.Cast().HideAllSections();
    }
}
