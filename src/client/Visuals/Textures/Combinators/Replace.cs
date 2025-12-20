// <copyright file="Replace.cs" company="VoxelGame">
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

using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Combinators;

/// <summary>
///     Replaces the pixels of the lower sheet with those of the upper sheet.
///     If the upper pixel transparency is <c>0</c>, the lower pixel is kept.
/// </summary>
[UsedImplicitly]
public class Replace() : BasicCombinator("replace")
{
    /// <inheritdoc />
    protected override void Apply(Image back, Image front)
    {
        for (var x = 0; x < back.Width; x++)
        for (var y = 0; y < back.Height; y++)
        {
            Color32 frontColor = front.GetPixel(x, y);

            if (frontColor.A == 0)
                continue;

            back.SetPixel(x, y, frontColor);
        }
    }
}
