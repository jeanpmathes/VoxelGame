// <copyright file="FluidAnimation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
///     Animates an image for use as a fluid texture.
/// </summary>
[UsedImplicitly]
public class FluidAnimation() : Modifier("fluid-animation", [invertedParameter])
{
    private const Int32 Moving = 3;
    private const Int32 MovingSide = 2;
    private const Int32 Static = 1;
    private const Int32 StaticSide = 0;

    private const Byte Frames = 16;

    private static readonly Parameter<Boolean> invertedParameter = CreateBooleanParameter("inverted", fallback: false);

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        Boolean inverted = parameters.Get(invertedParameter);

        if (context.Position == Vector2i.Zero && context.Size.Y != 4)
            context.ReportWarning($"Modifier {Type} expects a Nx4 sized sheet, but got {context.Size.X}x{context.Size.Y}");

        return context.Position.Y switch
        {
            Moving => GetLeftRightAnimation(image),
            MovingSide => GetFlowAnimation(image, inverted),
            Static => GetLeftRightAnimation(image),
            StaticSide => GetUpDownAnimation(image),
            _ => GetLeftRightAnimation(image)
        };
    }

    private static Sheet GetUpDownAnimation(Image image)
    {
        Sheet sheet = new(Frames, height: 1);
        Int32 pixelsPerFrame = image.Height / Frames;

        for (Byte frame = 0; frame < Frames; frame++)
        {
            Int32 offset = frame < Frames / 2 ? frame : Frames - frame - 1;

            sheet[frame, y: 0] = image.Translated(dx: 0, offset * pixelsPerFrame);
        }

        return sheet;
    }

    private static Sheet GetLeftRightAnimation(Image image)
    {
        Sheet sheet = new(Frames, height: 1);
        Int32 pixelsPerFrame = image.Width / Frames;

        for (Byte frame = 0; frame < Frames; frame++)
        {
            Int32 offset = frame < Frames / 2 ? frame : Frames - frame - 1;

            sheet[frame, y: 0] = image.Translated(offset * pixelsPerFrame, dy: 0);
        }

        return sheet;
    }

    private static Sheet GetFlowAnimation(Image image, Boolean inverted)
    {
        Sheet sheet = new(Frames, height: 1);
        Int32 pixelsPerFrame = image.Height / Frames;

        for (Byte frame = 0; frame < Frames; frame++)
        {
            Int32 offset = inverted ? Frames - frame - 1 : frame;

            sheet[frame, y: 0] = image.Translated(dx: 0, offset * pixelsPerFrame);
        }

        return sheet;
    }
}
