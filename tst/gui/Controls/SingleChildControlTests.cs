// <copyright file="SingleChildControlTests.cs" company="VoxelGame">
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

using JetBrains.Annotations;
using VoxelGame.GUI.Controls.Internals;
using Xunit;

namespace VoxelGame.GUI.Tests.Controls;

[TestSubject(typeof(SingleChildControl<>))]
public class SingleChildControlTests
{
    [Fact]
    public void SingleChildControl_Child_ShouldBeNullByDefault()
    {
        MockSingleChildControl control = new();

        Assert.Null(control.Child);
        Assert.Empty(control.Children);
    }

    [Fact]
    public void SingleChildControl_Child_ShouldUnparentOldChildWhenSet()
    {
        MockSingleChildControl control = new();
        MockControl child1 = new();
        MockControl child2 = new();

        control.Child = child1;

        Assert.Equal(child1, control.Child);
        Assert.Single(control.Children);
        Assert.Equal(control, child1.Parent.GetValue());

        control.Child = child2;

        Assert.Equal(child2, control.Child);
        Assert.Single(control.Children);
        Assert.Equal(control, child2.Parent.GetValue());
        Assert.Null(child1.Parent.GetValue());
    }
}
