// <copyright file="SlotTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Tests.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Bindings;

[TestSubject(typeof(Slot<>))]
public class SlotTests
{
    private readonly Slot<Int32> slot;
    private readonly EventObserver observer = new();

    public SlotTests()
    {
        slot = new Slot<Int32>(value: 10, this);
    }

    [Fact]
    public void Slot_SetValue_ShouldRaiseValueChangedWhenNewValueIsDifferent()
    {
        slot.ValueChanged += observer.OnEvent;
        slot.SetValue(9);

        Assert.Equal(expected: 9, slot.GetValue());
        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void SetValue_WithEqualValue_DoesNotRaiseValueChanged()
    {
        slot.ValueChanged += observer.OnEvent;
        slot.SetValue(10);

        Assert.Equal(expected: 10, slot.GetValue());
        Assert.Equal(expected: 0, observer.InvocationCount);
    }
}
