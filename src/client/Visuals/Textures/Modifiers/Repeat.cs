// <copyright file="Repeat.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
///     Repeats each image of a sheet the specified amount of times.
///     Makes more sense to be applied to single images.
/// </summary>
[UsedImplicitly]
public class Repeat() : Modifier("repeat", [xParameter, yParameter])
{
    private static readonly Parameter<Int32> xParameter = CreateIntegerParameter("x", fallback: 1);
    private static readonly Parameter<Int32> yParameter = CreateIntegerParameter("y", fallback: 1);

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        Int32 xRepeat = parameters.Get(xParameter);
        Int32 yRepeat = parameters.Get(yParameter);

        if (xRepeat < 1 || yRepeat < 1)
            context.ReportWarning("Repeat parameters are not positive");

        if (xRepeat > Byte.MaxValue || yRepeat > Byte.MaxValue)
            context.ReportWarning("Repeat parameters exceed maximum value");

        Sheet result = new((Byte) xRepeat, (Byte) yRepeat);

        for (Byte x = 0; x < xRepeat; x++)
        for (Byte y = 0; y < yRepeat; y++)
            result[x, y] = image.CreateCopy();

        return result;
    }
}
