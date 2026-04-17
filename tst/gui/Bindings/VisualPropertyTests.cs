// <copyright file="VisualPropertyTests.cs" company="VoxelGame">
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
using System.Drawing;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Tests.Utilities;
using VoxelGame.GUI.Tests.Visuals;
using VoxelGame.GUI.Visuals;
using Xunit;

namespace VoxelGame.GUI.Tests.Bindings;

public class VisualPropertyTests
{
    private readonly MockVisual visual = new();
    private readonly EventObserver observer = new();

    private readonly Slot<Int32> source;

    public VisualPropertyTests()
    {
        source = new Slot<Int32>(value: 0, this);
    }

    [Fact]
    public void VisualProperty_MeasureInvalidation_ShouldInvalidateMeasure()
    {
        VisualProperty<Single> property = VisualProperty.Create(visual, defaultValue: 1f, Invalidation.Measure);

        property.Activate();
        visual.Measure(new SizeF(width: 100, height: 100));
        Int32 firstCount = visual.MeasureCalls;

        property.Value = 2f;
        visual.Measure(new SizeF(width: 100, height: 100));

        Assert.Equal(firstCount + 1, visual.MeasureCalls);
    }

    [Fact]
    public void VisualProperty_ArrangeInvalidation_ShouldInvalidateArrange()
    {
        VisualProperty<Single> property = VisualProperty.Create(visual, defaultValue: 1f, Invalidation.Arrange);

        property.Activate();
        visual.Measure(new SizeF(width: 100, height: 100));
        visual.Arrange(new RectangleF(x: 0, y: 0, width: 100, height: 100));
        Int32 firstCount = visual.ArrangeCalls;

        property.Value = 2f;
        visual.Arrange(new RectangleF(x: 0, y: 0, width: 100, height: 100));

        Assert.Equal(firstCount + 1, visual.ArrangeCalls);
    }

    [Fact]
    public void VisualProperty_Binding_ShouldRaiseValueChangedEventWhenActive()
    {
        VisualProperty<Int32> property = VisualProperty.Create(visual, Binding.To(source), Invalidation.None);

        property.ValueChanged += observer.OnEvent;

        property.Activate();
        source.SetValue(1);
        source.SetValue(2);

        Assert.Equal(expected: 2, property.GetValue());
        Assert.Equal(expected: 3, observer.InvocationCount);
    }
}
