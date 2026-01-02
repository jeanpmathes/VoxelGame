// <copyright file="NoTint.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
///     Transforms the layer into the no-tint variant, changing the alpha channel to a fitting value.
/// </summary>
[UsedImplicitly]
public class NoTint() : Modifier("no-tint")
{
    private const Byte Alpha = Byte.MaxValue / 4;

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Color32 color = image.GetPixel(x, y);

            if (color.A == 0)
                continue;

            image.SetPixel(x, y, color with {A = Alpha});
        }

        return Wrap(image);
    }
}
