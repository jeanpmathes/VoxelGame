// <copyright file="ControlTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Tests.Input;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Tests.Utilities;
using VoxelGame.GUI.Tests.Visuals;
using VoxelGame.GUI.Themes;
using Xunit;

namespace VoxelGame.GUI.Tests.Controls;

[TestSubject(typeof(Control))]
public class ControlTests
{
    [Fact]
    public void Control_ShouldApplyStyleWhenControlGetsVisualized()
    {
        ThemeBuilder builder = new();
        builder.AddStyle<Border>("", s => s.Set(c => c.MinimumWidth, value: 25f));

        using Canvas canvas = Canvas.Create(new MockRenderer(), builder.BuildTheme());
        Border border = new();

        canvas.Child = border;

        Assert.Equal(expected: 25f, border.MinimumWidth.GetValue());
    }

    [Fact]
    public void Control_ShouldApplyLocalStyleAfterContextStyle()
    {
        ThemeBuilder builder = new();
        builder.AddStyle<Border>("", s => s.Set(c => c.MinimumWidth, value: 50f));

        using Canvas canvas = Canvas.Create(new MockRenderer(), builder.BuildTheme());

        Border border = new()
        {
            Style =
            {
                Value = Styling.Create<Border>("", s => s.Set(c => c.MinimumWidth, value: 10f))
            }
        };

        canvas.Child = border;

        Assert.Equal(expected: 10f, border.MinimumWidth.GetValue());
    }

    [Fact]
    public void Control_Template_ShouldUpdateVisualizationWhenChanged()
    {
        using Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());

        MockControl control = new();

        EventObserver listener1 = new();
        EventObserver listener2 = new();

        control.Template.Value = ControlTemplate.Create<MockControl>(_ =>
        {
            listener1.OnAction();
            return new MockVisual();
        });

        canvas.Child = control;

        control.Template.Value = ControlTemplate.Create<MockControl>(_ =>
        {
            listener2.OnAction();
            return new MockVisual();
        });

        Assert.Equal(expected: 1, listener1.InvocationCount);
        Assert.Equal(expected: 1, listener2.InvocationCount);
    }

    [Fact]
    public void Control_Visibility_ShouldNotBeHigherThanParentVisibility()
    {
        using Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());

        MockControl child = new();
        canvas.Child = child;

        canvas.Visibility.Value = Visibility.Hidden;

        Assert.Equal(Visibility.Hidden, child.Visibility.GetValue());
    }

    [Fact]
    public void Control_IsHovered_ShouldBeTrueWhenPointerIsOverControl()
    {
        using Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());

        MockControl control = new();
        canvas.Child = control;
        canvas.SetRenderingSize(new Size(width: 500, height: 500));

        canvas.MovePointerTo(control);

        Assert.True(control.IsHovered.GetValue());

        canvas.MovePointerTo(new PointF(x: -10f, y: -10f));

        Assert.False(control.IsHovered.GetValue());
    }
}
