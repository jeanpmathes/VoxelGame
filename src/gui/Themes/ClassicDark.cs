// <copyright file="ClassicDark.cs" company="VoxelGame">
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

using System.Drawing;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Styles;
using Brush = VoxelGame.GUI.Graphics.Brush;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VoxelGame.GUI.Themes;

/// <summary>
///     The original dark GWEN theme, formerly known as "DefaultSkin2". It is a dark theme, with a more modern look.
/// </summary>
public class ClassicDark(ThemeBuilder builder) // todo: remove, rethink
{
    private static readonly Brush basicBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 45, green: 45, blue: 48));
    private static readonly Brush basicForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 255, green: 255, blue: 255));

    private static readonly Brush hoveredBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 62, green: 62, blue: 64));
    private static readonly Brush hoveredForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 122, blue: 204));

    private static readonly Brush pressedBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 122, blue: 204));
    private static readonly Brush pressedForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 255, green: 255, blue: 255));

    private static readonly Brush disabledForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 62, green: 62, blue: 64));

    private static readonly Brush focusedOutlineBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 0, blue: 0));

    public Style<Canvas> CanvasStyle { get; } = builder.AddStyle<Canvas>(nameof(CanvasStyle),
        b => b
            .Set(x => x.Foreground, basicForegroundBrush)
            .Set(x => x.Background, basicBackgroundBrush)
    );

    public Style<IButton> ButtonStyle { get; } = builder.AddStyle<IButton>(nameof(ButtonStyle),
        b => b
            .Set(x => x.Opacity, value: 1.0f)
            .Set(x => x.BorderBrush, basicBackgroundBrush)
            .Trigger(x => x.IsHovered, x => x.Background, hoveredBackgroundBrush)
            .Trigger(x => x.IsPressed, x => x.Background, pressedBackgroundBrush)
            .Trigger(x => x.IsHovered, x => x.BorderBrush, hoveredBackgroundBrush)
            .Trigger(x => x.IsPressed, x => x.BorderBrush, pressedBackgroundBrush)
            .Trigger(x => x.IsKeyboardFocused, x => x.BorderBrush, focusedOutlineBrush)
            .Trigger(x => x.IsKeyboardFocused, x => x.BorderStrokeStyle, StrokeStyle.Squared)
            .Trigger(x => x.IsHovered, x => x.Foreground, hoveredForegroundBrush)
            .Trigger(x => x.IsPressed, x => x.Foreground, pressedForegroundBrush)
            .Trigger(x => Binding.To(x.Enablement).Compute(v => v.IsDisabled), x => x.Foreground, disabledForegroundBrush)
    );
}
