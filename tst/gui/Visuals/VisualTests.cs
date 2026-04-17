// <copyright file="VisualTests.cs" company="VoxelGame">
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
using Xunit;

namespace VoxelGame.GUI.Tests.Visuals;

public class VisualTests
{
    private readonly MockVisual visual = new();

    [Fact]
    public void Visual_Arrange_ShouldUpdateBoundsWhenAvailableSizeChanges()
    {
        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 100f, height: 100f));
        visual.Arrange(new RectangleF(x: 20f, y: 30f, width: 40f, height: 50f));

        Assert.Equal(expected: 20f, visual.Bounds.X);
        Assert.Equal(expected: 30f, visual.Bounds.Y);
    }

    [Fact]
    public void Visual_Measure_ShouldClampToMinimumSize()
    {
        visual.MinimumWidth.Value = 50f;
        visual.MinimumHeight.Value = 30f;

        visual.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 50f, visual.MeasuredSize.Width);
        Assert.Equal(expected: 30f, visual.MeasuredSize.Height);
    }

    [Fact]
    public void Visual_Arrange_ShouldTakeMaximumSizeOverChildMinimumSize()
    {
        visual.MaximumWidth.Value = 50f;
        visual.MaximumHeight.Value = 40f;

        MockVisual child = new() {MinimumWidth = {Value = 100f}, MinimumHeight = {Value = 100f}};

        visual.SetChildVisual(child);

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 50f, visual.Bounds.Width);
        Assert.Equal(expected: 40f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Measure_ShouldIncludeMargin()
    {
        visual.Margin.Value = new ThicknessF(left: 5f, top: 10f, right: 15f, bottom: 20f);

        visual.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 21f, visual.MeasuredSize.Width);
        Assert.Equal(expected: 31f, visual.MeasuredSize.Height);
    }

    [Fact]
    public void Visual_Arrange_ShouldOffsetBoundsByMargin()
    {
        visual.Margin.Value = new ThicknessF(left: 5f, top: 10f, right: 15f, bottom: 20f);

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 5f, visual.Bounds.X);
        Assert.Equal(expected: 10f, visual.Bounds.Y);
    }

    [Fact]
    public void Visual_Arrange_ShouldNotIncludeSizeOfMargin()
    {
        visual.Margin.Value = new ThicknessF(left: 5f, top: 10f, right: 15f, bottom: 20f);

        visual.Measure(new SizeF(width: 200f, height: 200f));
        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 180f, visual.Bounds.Width);
        Assert.Equal(expected: 170f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Measure_ShouldIncludePadding()
    {
        visual.Margin.Value = new ThicknessF(left: 5f, top: 10f, right: 15f, bottom: 20f);
        visual.SetChildVisual(new MockVisual());

        visual.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 21f, visual.MeasuredSize.Width);
        Assert.Equal(expected: 31f, visual.MeasuredSize.Height);
    }

    [Fact]
    public void Visual_Measure_ShouldSizeHiddenVisualsAsNormal()
    {
        visual.Visibility.Value = Visibility.Hidden;

        visual.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 1f, visual.MeasuredSize.Width);
        Assert.Equal(expected: 1f, visual.MeasuredSize.Height);
    }

    [Fact]
    public void Visual_Measure_ShouldSizeCollapsedVisualsAsEmpty()
    {
        visual.Visibility.Value = Visibility.Collapsed;

        visual.Measure(new SizeF(width: 200f, height: 200f));

        Assert.Equal(expected: 0f, visual.MeasuredSize.Width);
        Assert.Equal(expected: 0f, visual.MeasuredSize.Height);
    }

    [Fact]
    public void Visual_Arrange_ShouldSetBoundsOfCollapsedVisualToEmpty()
    {
        visual.Visibility.Value = Visibility.Collapsed;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 0f, visual.Bounds.Width);
        Assert.Equal(expected: 0f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Arrange_HorizontalStretch_ShouldFillAvailableWidth()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Stretch;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 0f, visual.Bounds.X);
        Assert.Equal(expected: 200f, visual.Bounds.Width);
    }

    [Fact]
    public void Visual_Arrange_Left_ShouldAlignToLeft()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Left;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 0f, visual.Bounds.X);
        Assert.Equal(expected: 1f, visual.Bounds.Width);
    }

    [Fact]
    public void Visual_Arrange_HorizontalCenter_ShouldCenterInAvailableSpace()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Center;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 99.5f, visual.Bounds.X);
        Assert.Equal(expected: 1f, visual.Bounds.Width);
    }

    [Fact]
    public void Visual_Arrange_Right_ShouldAlignToRight()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Right;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 199f, visual.Bounds.X);
        Assert.Equal(expected: 1f, visual.Bounds.Width);
    }

    [Fact]
    public void Visual_Arrange_ShouldCenterWithinMargins()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Center;
        visual.Margin.Value = new ThicknessF(left: 10f, top: 0f, right: 20f, bottom: 0f);

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 94.5f, visual.Bounds.X);
        Assert.Equal(expected: 1f, visual.Bounds.Width);
    }

    [Fact]
    public void Visual_Arrange_VerticalStretch_ShouldFillAvailableHeight()
    {
        visual.VerticalAlignment.Value = VerticalAlignment.Stretch;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 0f, visual.Bounds.Y);
        Assert.Equal(expected: 200f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Arrange_Top_ShouldAlignToTop()
    {
        visual.VerticalAlignment.Value = VerticalAlignment.Top;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 0f, visual.Bounds.Y);
        Assert.Equal(expected: 1f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Arrange_VerticalCenter_ShouldCenterInAvailableSpace()
    {
        visual.VerticalAlignment.Value = VerticalAlignment.Center;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 99.5f, visual.Bounds.Y);
        Assert.Equal(expected: 1f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Arrange_Bottom_ShouldAlignToBottom()
    {
        visual.VerticalAlignment.Value = VerticalAlignment.Bottom;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 200f));

        Assert.Equal(expected: 199f, visual.Bounds.Y);
        Assert.Equal(expected: 1f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Arrange_ShouldCenterOnBothAxesWhenRequested()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Center;
        visual.VerticalAlignment.Value = VerticalAlignment.Center;
        visual.MinimumWidth.Value = 40f;
        visual.MinimumHeight.Value = 20f;

        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 100f));

        Assert.Equal(expected: 80f, visual.Bounds.X);
        Assert.Equal(expected: 40f, visual.Bounds.Y);
        Assert.Equal(expected: 40f, visual.Bounds.Width);
        Assert.Equal(expected: 20f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_Arrange_ShouldPositionAtBottomRightWhenRequested()
    {
        visual.HorizontalAlignment.Value = HorizontalAlignment.Right;
        visual.VerticalAlignment.Value = VerticalAlignment.Bottom;
        visual.MinimumWidth.Value = 40f;
        visual.MinimumHeight.Value = 20f;

        visual.Measure(new SizeF(width: 200f, height: 100f));
        visual.Arrange(new RectangleF(x: 0f, y: 0f, width: 200f, height: 100f));

        Assert.Equal(expected: 160f, visual.Bounds.X);
        Assert.Equal(expected: 80f, visual.Bounds.Y);
        Assert.Equal(expected: 40f, visual.Bounds.Width);
        Assert.Equal(expected: 20f, visual.Bounds.Height);
    }

    [Fact]
    public void Visual_LocalPointToRoot_ShouldAddParentOffset()
    {
        MockVisual child = new();
        visual.SetChildVisual(child);

        visual.Arrange(new RectangleF(x: 10f, y: 20f, width: 200f, height: 200f));

        PointF rootPoint = child.LocalPointToRoot(new PointF(x: 5f, y: 5f));

        Assert.Equal(expected: 15f, rootPoint.X);
        Assert.Equal(expected: 25f, rootPoint.Y);
    }

    [Fact]
    public void Visual_RootPointToLocal_ShouldSubtractParentOffset()
    {
        MockVisual child = new();
        visual.SetChildVisual(child);

        visual.Arrange(new RectangleF(x: 10f, y: 20f, width: 200f, height: 200f));

        PointF localPoint = child.RootPointToLocal(new PointF(x: 15f, y: 25f));

        Assert.Equal(expected: 5f, localPoint.X);
        Assert.Equal(expected: 5f, localPoint.Y);
    }

    [Fact]
    public void Visual_GetChildAfter_ShouldReturnNullWhenPassedVisualIsNotAChild()
    {
        visual.CreateWideChildHierarchy(width: 3);

        MockVisual other = new();

        Assert.Null(visual.GetChildAfter(other));
    }

    [Fact]
    public void Visual_GetChildAfter_ShouldReturnNullIfThereIsOnlyOneChild()
    {
        MockVisual child = new();
        visual.AddChildVisual(child);

        Assert.Null(visual.GetChildAfter(child));
    }

    [Fact]
    public void Visual_GetChildAfter_ShouldReturnNextChild()
    {
        MockVisual first = new();
        MockVisual second = new();

        visual.AddChildVisual(first);
        visual.AddChildVisual(second);

        Assert.Same(second, visual.GetChildAfter(first));
    }

    [Fact]
    public void Visual_GetChildBefore_ShouldReturnNullWhenPassedVisualIsNotAChild()
    {
        visual.CreateWideChildHierarchy(width: 3);

        MockVisual other = new();

        Assert.Null(visual.GetChildBefore(other));
    }

    [Fact]
    public void Visual_GetChildBefore_ShouldReturnNullIfThereIsOnlyOneChild()
    {
        MockVisual child = new();
        visual.AddChildVisual(child);

        Assert.Null(visual.GetChildBefore(child));
    }

    [Fact]
    public void Visual_GetChildBefore_ShouldReturnPreviousChild()
    {
        MockVisual first = new();
        MockVisual second = new();

        visual.AddChildVisual(first);
        visual.AddChildVisual(second);

        Assert.Same(first, visual.GetChildBefore(second));
    }
}
