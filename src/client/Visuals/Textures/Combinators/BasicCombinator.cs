// <copyright file="BasicCombinator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Combinators;

/// <summary>
/// Implements the basic functionality of a combinator, allowing easy creation of new combinators.
/// Will support 1-to-1, 1-to-all, and all-to-all operations.
/// </summary>
public abstract class BasicCombinator(String type) : Combinator(type)
{
    /// <inheritdoc />
    public sealed override Sheet? Combine(Sheet current, Sheet next, IContext context)
    {
        Boolean oneToAll = next.IsSingle && current is {Width: > 1, Height: > 1};
        Boolean equal = current.Width == next.Width && current.Height == next.Height;

        if (oneToAll)
            return ApplyOneToAll(current, next);

        if (equal)
            return ApplyOneToOne(current, next);

        context.ReportWarning(
            $"The '{Type}' combinator can either combine a single image on any sheet, " +
            $"or two sheets of the same size - not {next.Width}x{next.Height} on {current.Width}x{current.Height}");

        return null;
    }

    private Sheet ApplyOneToAll(Sheet back, Sheet front)
    {
        for (Byte x = 0; x < back.Width; x++)
        for (Byte y = 0; y < back.Height; y++)
            Apply(back[x, y], front[x: 0, y: 0]);

        return back;
    }

    private Sheet ApplyOneToOne(Sheet back, Sheet front)
    {
        for (Byte x = 0; x < back.Width; x++)
        for (Byte y = 0; y < back.Height; y++)
            Apply(back[x, y], front[x, y]);

        return back;
    }

    /// <summary>
    /// Apply the combination logic to two images.
    /// </summary>
    /// <param name="back">The image to be modified.</param>
    /// <param name="front">The image to modify with.</param>
    protected abstract void Apply(Image back, Image front);
}
