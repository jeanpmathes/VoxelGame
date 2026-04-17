// <copyright file="LinearLayoutTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Utilities;
using VoxelGame.GUI.Visuals;
using Xunit;

namespace VoxelGame.GUI.Tests.Visuals;

public class LinearLayoutTests() : VisualTestBase<LinearLayout>(() => new LinearLayout())
{
    private readonly MockLinearLayout layout = new() {Visibility = {Value = Visibility.Visible}};

    [Fact]
    public void LinearLayout_Measure_NoChildren_ShouldSetSizeToMinimumSize()
    {
        layout.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 1f, layout.MeasuredSize.Width);
        Assert.Equal(expected: 1f, layout.MeasuredSize.Height);
    }

    [Fact]
    public void LinearLayout_Measure_OneChild_ShouldFitChildSize()
    {
        layout.Add(new MockVisual {MinimumWidth = {Value = 50f}, MinimumHeight = {Value = 30f}});

        layout.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 50f, layout.MeasuredSize.Width);
        Assert.Equal(expected: 30f, layout.MeasuredSize.Height);
    }

    [Fact]
    public void LinearLayout_Measure_MultipleChildrenHorizontal_ShouldSumWidthAndUseMaximumHeight()
    {
        layout.Add(new MockVisual {MinimumWidth = {Value = 40f}, MinimumHeight = {Value = 20f}});
        layout.Add(new MockVisual {MinimumWidth = {Value = 60f}, MinimumHeight = {Value = 50f}});
        layout.Add(new MockVisual {MinimumWidth = {Value = 30f}, MinimumHeight = {Value = 10f}});

        layout.Measure(new SizeF(width: 500f, height: 500f));

        Assert.Equal(expected: 130f, layout.MeasuredSize.Width);
        Assert.Equal(expected: 50f, layout.MeasuredSize.Height);
    }

    [Fact]
    public void LinearLayout_Measure_WithPadding_ShouldIncludePaddingInFinalSize()
    {
        layout.Padding.Value = new ThicknessF(left: 10f, top: 5f, right: 10f, bottom: 5f);
        layout.Add(new MockVisual {MinimumWidth = {Value = 50f}, MinimumHeight = {Value = 30f}});

        layout.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 70f, layout.MeasuredSize.Width);
        Assert.Equal(expected: 40f, layout.MeasuredSize.Height);
    }

    [Fact]
    public void LinearLayout_Measure_MultipleChildrenVertical_ShouldSumHeightAndUseMaximumWidth()
    {
        layout.Orientation.Value = Orientation.Vertical;

        layout.Add(new MockVisual {MinimumWidth = {Value = 40f}, MinimumHeight = {Value = 20f}});
        layout.Add(new MockVisual {MinimumWidth = {Value = 60f}, MinimumHeight = {Value = 50f}});
        layout.Add(new MockVisual {MinimumWidth = {Value = 30f}, MinimumHeight = {Value = 10f}});

        layout.Measure(new SizeF(width: 500f, height: 500f));

        Assert.Equal(expected: 60f, layout.MeasuredSize.Width);
        Assert.Equal(expected: 80f, layout.MeasuredSize.Height);
    }

    [Fact]
    public void LinearLayout_Arrange_Horizontal_ShouldArrangeChildrenLeftToRight()
    {
        MockVisual child1 = new() {MinimumWidth = {Value = 40f}, MinimumHeight = {Value = 20f}};
        MockVisual child2 = new() {MinimumWidth = {Value = 60f}, MinimumHeight = {Value = 30f}};

        layout.Add(child1);
        layout.Add(child2);

        layout.Measure(new SizeF(width: 200f, height: 100f));
        layout.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 100f));

        Assert.Equal(expected: 0f, child1.Bounds.X);
        Assert.Equal(expected: 40f, child2.Bounds.X);

        Assert.Equal(expected: 40f, child1.Bounds.Width);
        Assert.Equal(expected: 100f, child1.Bounds.Height);

        Assert.Equal(expected: 60f, child2.Bounds.Width);
        Assert.Equal(expected: 100f, child2.Bounds.Height);
    }

    [Fact]
    public void LinearLayout_Arrange_Vertical_ShouldArrangeChildrenTopToBottom()
    {
        layout.Orientation.Value = Orientation.Vertical;

        MockVisual child1 = new() {MinimumWidth = {Value = 40f}, MinimumHeight = {Value = 20f}};
        MockVisual child2 = new() {MinimumWidth = {Value = 60f}, MinimumHeight = {Value = 30f}};

        layout.Add(child1);
        layout.Add(child2);

        layout.Measure(new SizeF(width: 200f, height: 200f));
        layout.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 0f, child1.Bounds.Y);
        Assert.Equal(expected: 20f, child2.Bounds.Y);

        Assert.Equal(expected: 200f, child1.Bounds.Width);
        Assert.Equal(expected: 20f, child1.Bounds.Height);

        Assert.Equal(expected: 200f, child2.Bounds.Width);
        Assert.Equal(expected: 30f, child2.Bounds.Height);
    }

    private class MockLinearLayout : LinearLayout
    {
        public void Add(Visual child)
        {
            AddChild(child);
        }
    }
}
