// <copyright file="Alpha.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
/// Sets the alpha value of the image to the given value.
/// </summary>
[UsedImplicitly]
public class Alpha() : Modifier("alpha", [valueParameter])
{
    private static readonly Parameter<Double> valueParameter = CreateDoubleParameter("value");

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters)
    {
        Double alpha = parameters.Get(valueParameter);

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Vector4d pixel = image.GetPixel(x, y).ToVector4();

            pixel.W = alpha;

            image.SetPixel(x, y, pixel.ToColor());
        }

        return Wrap(image);
    }
}
