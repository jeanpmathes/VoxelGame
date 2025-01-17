// <copyright file="Colorize.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
/// Applies a color tint to the layer.
/// </summary>
[UsedImplicitly]
public class Colorize() : Modifier("colorize", [colorParameter])
{
    private static readonly Parameter<Color> colorParameter = CreateColorParameter("color");

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        Vector4d color = parameters.Get(colorParameter).ToVector4();

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Vector4d pixel = image.GetPixel(x, y).ToVector4();

            Vector4d result = pixel * color;
            result.W = pixel.W;

            image.SetPixel(x, y, result.ToColor());
        }

        return Wrap(image);
    }
}
