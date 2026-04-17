// <copyright file="PropertyTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Tests.Controls;
using VoxelGame.GUI.Tests.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Bindings;

public class PropertyTests
{
    private readonly MockControl owner = new();
    private readonly EventObserver observer = new();

    private readonly Slot<Int32> source;

    public PropertyTests()
    {
        source = new Slot<Int32>(value: 0, this);
    }

    [Fact]
    public void Property_GetValue_ShouldReturnDefaultValueIfNotBound()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 42);

        Assert.Equal(expected: 42, property.GetValue());
    }

    [Fact]
    public void Property_GetValue_ShouldGiveBindingHigherPrecedenceThanDefaultValue()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 42);

        property.Binding = Binding.To(source);

        Assert.Equal(expected: 0, property.GetValue());
    }

    [Fact]
    public void Property_GetValue_ShouldGiveLocalValueHigherPrecedenceThanDefaultValue()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 42);

        property.Value = 1337;

        Assert.Equal(expected: 1337, property.GetValue());
    }

    [Fact]
    public void Property_GetValue_ShouldGiveStyleHigherPrecedenceThanDefaultValue()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 42);

        property.Style(1337);

        Assert.Equal(expected: 1337, property.GetValue());
    }

    [Fact]
    public void Property_GetValue_ShouldGiveBindingHigherPrecedenceThanStyle()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 42);

        property.Binding = Binding.To(source);
        property.Style(1337);

        Assert.Equal(expected: 0, property.GetValue());
    }

    [Fact]
    public void Property_GetValue_ShouldGiveLocalValueHigherPrecedenceThanStyle()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 42);

        property.Value = 1337;
        property.Style(420);

        Assert.Equal(expected: 1337, property.GetValue());
    }

    [Fact]
    public void Property_ValueChanged_ShouldBeRaisedIfAndOnlyIfActive()
    {
        Property<Int32> property = Property.Create(owner, defaultValue: 0);
        property.Binding = Binding.To(source);

        property.ValueChanged += observer.OnEvent;

        source.SetValue(1);

        Assert.Equal(expected: 1, property.GetValue());
        Assert.Equal(expected: 0, observer.InvocationCount);

        property.Activate(); // Activation causes an event as well.
        source.SetValue(2);

        Assert.Equal(expected: 2, property.GetValue());
        Assert.Equal(expected: 2, observer.InvocationCount);

        observer.Reset();
        property.Deactivate();
        source.SetValue(3);

        Assert.Equal(expected: 3, property.GetValue());
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void Property_Coercion_ShouldBeAppliedToDefaultValue()
    {
        Binding<Int32, Int32> clamp = Binding.Computed<Int32, Int32>(input => Math.Clamp(input, min: 0, max: 3));
        Property<Int32> property = Property.Create(owner, defaultValue: 5, clamp);

        Assert.Equal(expected: 3, property.GetValue());
    }

    [Fact]
    public void Property_Coercion_ShouldBeAppliedToLocalValue()
    {
        Binding<Int32, Int32> clamp = Binding.Computed<Int32, Int32>(input => Math.Clamp(input, min: 0, max: 3));
        Property<Int32> property = Property.Create(owner, defaultValue: 5, clamp);

        property.Value = 7;

        Assert.Equal(expected: 3, property.GetValue());
    }

    [Fact]
    public void Property_Coercion_ShouldRaiseValueChangedWhenCoercionChangeCausesEffectiveValueChange()
    {
        Slot<Int32> maxSource = new(value: 10, this);

        Binding<Int32, Int32> clamp = Binding.To(maxSource).Parametrize<Int32, Int32>((input, max) => Math.Clamp(input, min: 0, max));
        Property<Int32> property = Property.Create(owner, defaultValue: 15, clamp);

        property.Activate();

        property.ValueChanged += observer.OnEvent;

        maxSource.SetValue(5);

        Assert.Equal(expected: 1, observer.InvocationCount);
        Assert.Equal(expected: 5, property.GetValue());
    }

    [Fact]
    public void Property_Coercion_ShouldNotRaiseValueChangedWhenCoercionChangeDoesNotCauseEffectiveValueChange()
    {
        Slot<Int32> maxSource = new(value: 10, this);

        Binding<Int32, Int32> clamp = Binding.To(maxSource).Parametrize<Int32, Int32>((input, max) => Math.Clamp(input, min: 0, max));
        Property<Int32> property = Property.Create(owner, defaultValue: 10, clamp);

        property.Activate();

        property.ValueChanged += observer.OnEvent;

        maxSource.SetValue(15);

        Assert.Equal(expected: 0, observer.InvocationCount);
        Assert.Equal(expected: 10, property.GetValue());
    }
}
