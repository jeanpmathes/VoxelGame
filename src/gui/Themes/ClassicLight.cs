// <copyright file="ClassicLight.cs" company="VoxelGame">
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
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VoxelGame.GUI.Themes;

/// <summary>
///     The original GWEN theme, formerly known as "DefaultSkin". It is a light theme with rounded corners.
/// </summary>
public class ClassicLight(ThemeBuilder builder)
{
    // todo: ClassicDark and ClassicLight will probably have very similar code, so creating a sort of shared theme constructor could be a good idea

    private static readonly Brush basicBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 253, green: 253, blue: 253));
    private static readonly Brush basicForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 73, green: 73, blue: 73));

    private static readonly Brush buttonBorderBrush = new SolidColorBrush(Color.FromArgb(red: 82, green: 82, blue: 82));
    private static readonly Brush buttonBackgroundBrush = basicBackgroundBrush; // todo: make gradient

    private static readonly Brush buttonHoveredBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 241, green: 241, blue: 241)); // todo: maybe make gradient
    private static readonly Brush buttonPressedBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 87, green: 180, blue: 245)); // todo: make gradient

    private static readonly Brush buttonDisabledForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 115, green: 115, blue: 115));
    private static readonly Brush buttonDisabledBorderBrush = new SolidColorBrush(Color.FromArgb(red: 154, green: 154, blue: 154));
    private static readonly Brush buttonDisabledBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 244, green: 244, blue: 244));

    public Style<Canvas> CanvasStyle { get; } = builder.AddStyle<Canvas>(nameof(CanvasStyle),
        b => b
            .Set(x => x.Foreground, basicForegroundBrush)
            .Set(x => x.Background, basicBackgroundBrush)
    );

    public Style<IButton> ButtonStyle { get; } = builder.AddStyle<IButton>(nameof(ButtonStyle),
        b => b
            .Set(x => x.Opacity, value: 1.0f)
            .Set(x => x.BorderBrush, buttonBorderBrush)
            .Set(x => x.BorderWidth, new WidthF(2.0f))
            .Set(x => x.BorderRadius, new RadiusF(5.0f))
            .Set(x => x.Background, buttonBackgroundBrush)
            .Trigger(x => x.IsHovered, x => x.Background, buttonHoveredBackgroundBrush)
            .Trigger(x => x.IsPressed, x => x.Background, buttonPressedBackgroundBrush)
            .Trigger(x => Binding.To(x.Enablement).Compute(v => v.IsDisabled), x => x.Background, buttonDisabledBackgroundBrush)
            .Trigger(x => Binding.To(x.Enablement).Compute(v => v.IsDisabled), x => x.BorderBrush, buttonDisabledBorderBrush)
            .Trigger(x => Binding.To(x.Enablement).Compute(v => v.IsDisabled), x => x.Foreground, buttonDisabledForegroundBrush)
    );
}
