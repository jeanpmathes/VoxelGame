// <copyright file="Alpha.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
/// Sets the alpha value of the image to the given value.
/// </summary>
[UsedImplicitly]
public class Alpha() : Modifier("alpha", [valueParameter])
{
    private static readonly Parameter<Double> valueParameter = CreateDoubleParameter("value");

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        var alpha = (Byte) (parameters.Get(valueParameter) * Byte.MaxValue);

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
            image.SetPixel(x, y, image.GetPixel(x, y) with {A = alpha});

        return Wrap(image);
    }
}
