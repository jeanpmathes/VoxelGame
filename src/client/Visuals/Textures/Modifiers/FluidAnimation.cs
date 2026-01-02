// <copyright file="FluidAnimation.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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

    private const Byte Frames = Constants.FluidAnimationFrames;

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
