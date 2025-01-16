// <copyright file="Blur.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using JetBrains.Annotations;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
/// Blurs images.
/// </summary>
[UsedImplicitly]
public class Blur() : Modifier("blur")
{
    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters)
    {
        Image blurred = new(image.Width, image.Height);

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Color pixel = GetBlurredPixel(image, x, y);

            blurred.SetPixel(x, y, pixel);
        }

        return Wrap(blurred);
    }

    private static Color GetBlurredPixel(Image image, Int32 x, Int32 y)
    {
        const Int32 size = 1;

        Int32 a = image.GetPixel(x, y).A;

        var r = 0;
        var g = 0;
        var b = 0;

        var total = 0;

        for (Int32 i = -size; i <= size; i++)
        for (Int32 j = -size; j <= size; j++)
        {
            if (x + i < 0 || x + i >= image.Width || y + j < 0 || y + j >= image.Height)
                continue;

            Color pixel = image.GetPixel(x + i, y + j);

            r += pixel.R;
            g += pixel.G;
            b += pixel.B;

            total++;
        }

        if (total != 0)
        {
            r /= total;
            g /= total;
            b /= total;
        }

        return Color.FromArgb(a, r, g, b);
    }
}
