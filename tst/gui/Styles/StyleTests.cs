// <copyright file="StyleTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Tests.Controls;
using Xunit;

namespace VoxelGame.GUI.Tests.Styles;

public class StyleTests
{
    private readonly MockControl element = new();

    [Fact]
    public void Style_Apply_ShouldSetStyledProperty()
    {
        Style<MockControl> style = Styling.Create<MockControl>("", s => s.Set(c => c.MinimumHeight, value: 12f));

        style.Apply(element);
        Assert.Equal(expected: 12.0f, element.MinimumHeight.GetValue());
    }

    [Fact]
    public void Style_Clear_ShouldUnsetStyledProperty()
    {
        Style<MockControl> style = Styling.Create<MockControl>("", s => s.Set(c => c.MinimumHeight, value: 12f));

        style.Apply(element);
        style.Clear(element);

        Assert.Equal(expected: 1.0f, element.MinimumHeight.GetValue());
    }

    [Fact]
    public void Style_Set_ShouldTrackBindingChanges()
    {
        Slot<Single> minimumHeight = new(value: 12f, this);

        Style<MockControl> style = Styling.Create<MockControl>("", s => s.Set(c => c.MinimumHeight, Binding.To(minimumHeight)));

        style.Apply(element);
        minimumHeight.SetValue(24f);

        Assert.Equal(expected: 24f, element.MinimumHeight.GetValue());
    }

    [Fact]
    public void Style_Trigger_ShouldConditionallySetAndUnsetProperty()
    {
        Slot<Boolean> isTriggered = new(value: false, this);

        Style<MockControl> style = Styling.Create<MockControl>("",
            s => s
                .Set(c => c.MinimumHeight, value: 10f)
                .Trigger(_ => Binding.To(isTriggered), c => c.MinimumHeight, value: 20f));

        style.Apply(element);

        Assert.Equal(expected: 10f, element.MinimumHeight.GetValue());

        isTriggered.SetValue(true);

        Assert.Equal(expected: 20f, element.MinimumHeight.GetValue());

        isTriggered.SetValue(false);

        Assert.Equal(expected: 10f, element.MinimumHeight.GetValue());
    }
}
