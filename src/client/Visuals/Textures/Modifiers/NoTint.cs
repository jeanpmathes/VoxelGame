// <copyright file="NoTint.cs" company="VoxelGame">
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
/// Transforms the layer into the no-tint variant, changing the alpha channel to a fitting value.
/// </summary>
[UsedImplicitly]
public class NoTint() : Modifier("no-tint")
{
    private const Int32 Alpha = Byte.MaxValue / 4;

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Color color = image.GetPixel(x, y);

            if (color.A == 0)
                continue;

            image.SetPixel(x, y, Color.FromArgb(Alpha, color.R, color.G, color.B));
        }

        return Wrap(image);
    }
}
