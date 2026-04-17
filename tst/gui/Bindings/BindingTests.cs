// <copyright file="BindingTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Tests.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Bindings;

public class BindingTests
{
    private readonly EventObserver observer = new();

    private readonly Slot<Int32> source1;
    private readonly Slot<Int32> source2;
    private readonly Slot<Int32> source3;

    public BindingTests()
    {
        source1 = new Slot<Int32>(value: 3, this);
        source2 = new Slot<Int32>(value: 4, this);
        source3 = new Slot<Int32>(value: 5, this);
    }

    [Fact]
    public void Binding_Constant_ShouldReturnConstantValue()
    {
        Binding<Int32> binding = Binding.Constant(42);

        Assert.Equal(expected: 42, binding.GetValue());
    }

    [Fact]
    public void Binding_To_ShouldRaiseValueChangedEventWhenSourceChanges()
    {
        Binding<Int32> binding = Binding.To(source1).Compute(value => value * 2);
        binding.ValueChanged += observer.OnEvent;

        source1.SetValue(5);

        Assert.Equal(expected: 10, binding.GetValue());
        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Binding_To_ShouldRaiseValueChangedEventWhenSourceChanges_WithTwoSources()
    {
        Binding<Int32> binding = Binding.To(source1, source2).Compute((v1, v2) => v1 * v2);
        binding.ValueChanged += observer.OnEvent;

        source1.SetValue(5);

        Assert.Equal(expected: 20, binding.GetValue());
        Assert.Equal(expected: 1, observer.InvocationCount);

        source2.SetValue(6);

        Assert.Equal(expected: 30, binding.GetValue());
        Assert.Equal(expected: 2, observer.InvocationCount);
    }

    [Fact]
    public void Binding_To_ShouldRaiseValueChangedEventWhenSourceChanges_WithThreeSources()
    {
        Binding<Int32> binding = Binding.To(source1, source2, source3).Compute((v1, v2, v3) => v1 * v2 * v3);
        binding.ValueChanged += observer.OnEvent;

        source1.SetValue(5);

        Assert.Equal(expected: 100, binding.GetValue());
        Assert.Equal(expected: 1, observer.InvocationCount);

        source2.SetValue(6);

        Assert.Equal(expected: 150, binding.GetValue());
        Assert.Equal(expected: 2, observer.InvocationCount);

        source3.SetValue(7);

        Assert.Equal(expected: 210, binding.GetValue());
        Assert.Equal(expected: 3, observer.InvocationCount);
    }

    [Fact]
    public void Binding_Select_ShouldReturnValueFromSelectedInnerSource()
    {
        Binding<Int32> binding = Binding.To(source1).Select(_ => source2);

        Assert.Equal(source2.GetValue(), binding.GetValue());
    }

    [Fact]
    public void Binding_Select_ShouldReturnValueFromCurrentlySelectedInnerSource()
    {
        Binding<Int32> binding = Binding.To(source1).Select(v => v != -1 ? source2 : source3);

        Assert.Equal(source2.GetValue(), binding.GetValue());

        source1.SetValue(-1);

        Assert.Equal(source3.GetValue(), binding.GetValue());
    }

    [Fact]
    public void Binding_Select_ShouldRaiseValueChangedEventWhenInnerSourceChanges()
    {
        Binding<Int32> binding = Binding.To(source1).Select(_ => source2);
        binding.ValueChanged += observer.OnEvent;

        source2.SetValue(99);

        Assert.Equal(expected: 99, binding.GetValue());
        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Binding_Select_ShouldRaiseValueChangedEventWhenOuterSourceChangesEffectiveValue()
    {
        Binding<Int32> binding = Binding.To(source1).Select(v => v != -1 ? source2 : source3);
        binding.ValueChanged += observer.OnEvent;

        source1.SetValue(-1);

        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Binding_Select_ShouldNotRaiseValueChangedEventWhenOuterSourceDoesNotChangesEffectiveValue()
    {
        source2.SetValue(99);
        source3.SetValue(99);

        Binding<Int32> binding = Binding.To(source1).Select(v => v != -1 ? source2 : source3);
        binding.ValueChanged += observer.OnEvent;

        source1.SetValue(-1);

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void Binding_Select_ShouldNotReactToOldInnerSelectedSource()
    {
        Binding<Int32> binding = Binding.To(source1).Select(v => v != -1 ? source2 : source3);

        source1.SetValue(-1);

        binding.ValueChanged += observer.OnEvent;
        source2.SetValue(20);

        Assert.Equal(expected: 5, binding.GetValue());
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void Binding_Select_ShouldReactToNewInnerSelectedSource()
    {
        Binding<Int32> binding = Binding.To(source1).Select(v => v != -1 ? source2 : source3);

        source1.SetValue(-1);

        binding.ValueChanged += observer.OnEvent;
        source3.SetValue(30);

        Assert.Equal(expected: 30, binding.GetValue());
        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Binding_Select_ShouldReturnDefaultWhenSelectorReturnsNull()
    {
        Binding<String> binding = Binding.To(source1).Select(_ => null, "default");

        Assert.Equal("default", binding.GetValue());
    }

    [Fact]
    public void Binding_Select_ShouldReturnDefaultWhenSelectorReturnsNullAfterOuterSourceChanges()
    {
        Slot<String> source = new("hello", this);

        Binding<String> binding = Binding.To(source1).Select(v => v != -1 ? source : null, "default");

        source1.SetValue(-1);

        Assert.Equal("default", binding.GetValue());
    }
}
