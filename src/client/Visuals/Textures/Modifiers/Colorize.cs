// <copyright file="Colorize.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
///     Applies a color tint to the layer.
/// </summary>
[UsedImplicitly]
public class Colorize() : Modifier("colorize", [colorParameter])
{
    private static readonly Parameter<ColorS> colorParameter = CreateColorParameter("color");

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        ColorS color = parameters.Get(colorParameter);

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Color32 pixel = image.GetPixel(x, y);

            image.SetPixel(x, y, (pixel.ToColorS() * color).ToColor32() with {A = pixel.A});
        }

        return Wrap(image);
    }
}
