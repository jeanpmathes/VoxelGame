// <copyright file="Blur.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
///     Blurs images.
/// </summary>
[UsedImplicitly]
public class Blur() : Modifier("blur")
{
    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        Image blurred = new(image.Width, image.Height);

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Color32 pixel = GetBlurredPixel(image, x, y);

            blurred.SetPixel(x, y, pixel);
        }

        return Wrap(blurred);
    }

    private static Color32 GetBlurredPixel(Image image, Int32 x, Int32 y)
    {
        const Int32 size = 1;

        Byte alpha = image.GetPixel(x, y).A;

        Vector4i sum = Vector4i.Zero;
        var count = 0;

        for (Int32 i = -size; i <= size; i++)
        for (Int32 j = -size; j <= size; j++)
        {
            if (x + i < 0 || x + i >= image.Width || y + j < 0 || y + j >= image.Height)
                continue;

            sum += image.GetPixel(x + i, y + j).ToVector4i();
            count += 1;
        }

        if (count != 0)
            sum /= count;

        return Color32.FromRGBA(sum) with {A = alpha};
    }
}
