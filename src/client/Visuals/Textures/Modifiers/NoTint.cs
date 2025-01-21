// <copyright file="NoTint.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
/// Transforms the layer into the no-tint variant, changing the alpha channel to a fitting value.
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
