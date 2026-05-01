// <copyright file="Button.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls.Bases;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Utilities;
using VoxelGame.GUI.Visuals;
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.GUI.Controls;

/// <summary>
///     Non-generic contract for buttons.
/// </summary>
public interface IButton : IContentControl
{
    /// <inheritdoc cref="Button{TContent}.BorderBrush" />
    public Property<Brush> BorderBrush { get; }

    /// <inheritdoc cref="Button{TContent}.BorderWidth" />
    public Property<WidthF> BorderWidth { get; }

    /// <inheritdoc cref="Button{TContent}.BorderRadius" />
    public Property<RadiusF> BorderRadius { get; }

    /// <inheritdoc cref="Button{TContent}.BorderStrokeStyle" />
    public Property<StrokeStyle> BorderStrokeStyle { get; }

    /// <inheritdoc cref="ButtonBase{TContent,TControl}.IsPressed" />
    public ReadOnlySlot<Boolean> IsPressed { get; }
}

/// <summary>
///     A content control that can be clicked to perform an action.
/// </summary>
public class Button<TContent> : ButtonBase<TContent, Button<TContent>>, IButton where TContent : class
{
    /// <summary>
    ///     Creates a new instance of the <see cref="Button{TContent}" /> class.
    /// </summary>
    public Button()
    {
        BorderBrush = Property.Create(this, Binding.To(Background).Combine(IsKeyboardFocused).Compute((background, isFocused) => isFocused ? Defaults.Button.FocusedBorderBrush : background));
        BorderWidth = Property.Create(this, new WidthF(1.0f));
        BorderRadius = Property.Create(this, Defaults.Radius);
        BorderStrokeStyle = Property.Create(this, Binding.To(IsKeyboardFocused).Compute(isFocused => isFocused ? StrokeStyle.Squared : StrokeStyle.Solid));

        Foreground.OverrideDefault(old => old
            .Combine(Enablement)
            .Compute(ComputeForegroundBrush));

        Background.OverrideDefault(old => old
            .Combine(Enablement, IsPressed, IsHovered)
            .Compute(ComputeBackgroundBrush));

        IsNavigable.OverrideDefault(defaultValue: true);

        Padding.OverrideDefault(new ThicknessF(3.0f));
    }

    private static Brush ComputeForegroundBrush(Brush foreground, Enablement enablement)
    {
        if (enablement.IsDisabled) return Defaults.DisabledForegroundBrush;

        return Defaults.InteractiveForegroundBrush;
    }

    private static Brush ComputeBackgroundBrush(Brush background, Enablement enablement, Boolean isPressed, Boolean isHovered)
    {
        if (enablement.IsDisabled) return Defaults.DisabledBackgroundBrush;
        if (isPressed) return Defaults.Button.PressedBackgroundBrush;
        if (isHovered) return Defaults.Button.HoveredBackgroundBrush;

        return Defaults.InteractiveBackgroundBrush;
    }

    /// <inheritdoc />
    protected override ControlTemplate<Button<TContent>> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<Button<TContent>>(static control => new Visuals.Border
        {
            BorderBrush = {Binding = Binding.To(control.BorderBrush)},
            BorderWidth = {Binding = Binding.To(control.BorderWidth)},
            BorderRadius = {Binding = Binding.To(control.BorderRadius)},
            BorderStrokeStyle = {Binding = Binding.To(control.BorderStrokeStyle)},

            Child = new ChildPresenter()
        });
    }

    #region PROPERTIES

    /// <summary>
    ///     The brush used to draw the border of the button.
    /// </summary>
    public Property<Brush> BorderBrush { get; }

    /// <summary>
    ///     The width of the button's border.
    /// </summary>
    public Property<WidthF> BorderWidth { get; }

    /// <summary>
    ///     The radius of the corners of the button's border.
    /// </summary>
    public Property<RadiusF> BorderRadius { get; }

    /// <summary>
    ///     The stroke style of the button's border.
    /// </summary>
    public Property<StrokeStyle> BorderStrokeStyle { get; }

    #endregion PROPERTIES
}
