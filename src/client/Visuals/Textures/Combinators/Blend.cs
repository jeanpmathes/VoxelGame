// <copyright file="Blend.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Combinators;

/// <summary>
///     Blends two sheets together using alpha blending.
/// </summary>
[UsedImplicitly]
public class Blend() : Combinator("blend")
{
    /// <inheritdoc />
    public override Sheet? Combine(Sheet current, Sheet next, IContext context)
    {
        Boolean oneToAll = next.IsSingle && current is {Width: > 1, Height: > 1};
        Boolean equal = current.Width == next.Width && current.Height == next.Height;

        if (oneToAll)
            return BlendOneToAll(current, next);

        if (equal)
            return BlendEqual(current, next);

        context.ReportWarning(
            $"The '{Type}' combinator can either combine a single image on any sheet, " +
            $"or two sheets of the same size - not {next.Width}x{next.Height} on {current.Width}x{current.Height}");

        return null;
    }

    private static Sheet BlendOneToAll(Sheet back, Sheet front)
    {
        for (Byte x = 0; x < back.Width; x++)
        for (Byte y = 0; y < back.Height; y++)
            BlendImage(back[x, y], front[x: 0, y: 0]);

        return back;
    }

    private static Sheet BlendEqual(Sheet back, Sheet front)
    {
        for (Byte x = 0; x < back.Width; x++)
        for (Byte y = 0; y < back.Height; y++)
            BlendImage(back[x, y], front[x, y]);

        return back;
    }

    private static void BlendImage(Image back, Image front)
    {
        for (var x = 0; x < back.Width; x++)
        for (var y = 0; y < back.Height; y++)
        {
            Vector4d backColor = back.GetPixel(x, y).ToVector4();
            Vector4d frontColor = front.GetPixel(x, y).ToVector4();

            Vector4d result = backColor * (1 - frontColor.W) + frontColor * frontColor.W;

            back.SetPixel(x, y, result.ToColor());
        }
    }
}
