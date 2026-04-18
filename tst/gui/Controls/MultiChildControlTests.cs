// <copyright file="MultiChildControlTests.cs" company="VoxelGame">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Internals;
using VoxelGame.GUI.Tests.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Controls;

[TestSubject(typeof(MultiChildControl<>))]
public class MultiChildControlTests
{
    [Fact]
    public void MultiChildControl_Children_ShouldBeEmptyByDefault()
    {
        MockMultiChildControl control = new();

        Assert.Empty(control.Children);
    }

    [Fact]
    public void MultiChildControl_Children_ShouldBeAbleToContainMultipleChildren()
    {
        MockMultiChildControl control = new();
        MockControl firstChild = new();
        MockControl secondChild = new();

        control.Children.Add(firstChild);
        control.Children.Add(secondChild);

        Assert.Equal(expected: 2, control.Children.Count);
        Assert.Contains(firstChild, control.Children);
        Assert.Contains(secondChild, control.Children);
    }

    [Fact]
    public void MultiChildControl_Add_ShouldSetParentOfChild()
    {
        MockMultiChildControl control = new();
        MockControl child = new();

        control.Children.Add(child);

        Assert.Equal(control, child.Parent.GetValue());
    }

    [Fact]
    public void MultiChildControl_Clear_ShouldUnsetParentOfChild()
    {
        MockMultiChildControl control = new();
        MockControl child = new();

        control.Children.Add(child);
        control.Children.Remove(child);

        Assert.Null(child.Parent.GetValue());
        Assert.Empty(control.Children);
    }

    [Fact]
    public void MultiChildControl_Add_ShouldRaiseChildAddedEvent()
    {
        MockMultiChildControl control = new();
        MockControl child = new();

        EventObserver observer = new();
        control.ChildAdded += observer.OnEvent;

        control.Children.Add(child);

        Assert.Equal(expected: 1, observer.InvocationCount);
        Assert.Same(child, (observer.LastArgs as ChildAddedEventArgs)?.Child);
    }

    [Fact]
    public void MultiChildControl_Remove_ShouldRaiseChildRemovedEvent()
    {
        MockMultiChildControl control = new();
        MockControl child = new();

        EventObserver observer = new();
        control.ChildRemoved += observer.OnEvent;

        control.Children.Add(child);
        control.Children.Remove(child);

        Assert.Equal(expected: 1, observer.InvocationCount);
        Assert.Same(child, (observer.LastArgs as ChildRemovedEventArgs)?.Child);
    }

    [Fact]
    public void MultiChildControl_Set_ShouldReplaceAllChildren()
    {
        MockMultiChildControl control = new();
        MockControl oldChild = new();
        MockControl newChild1 = new();
        MockControl newChild2 = new();

        control.Children.Add(oldChild);
        control.Children = [newChild1, newChild2];

        Assert.Equal(expected: 2, control.Children.Count);
        Assert.Contains(newChild1, control.Children);
        Assert.Contains(newChild2, control.Children);
        Assert.Null(oldChild.Parent.GetValue());
    }

    [Fact]
    public void MultiChildControl_Set_ShouldBindToAssignedListSlot()
    {
        MockMultiChildControl control = new();

        MockControl child1 = new();
        MockControl child2 = new();

        ListSlot<Control> externalChildren = [child1];

        control.Children = externalChildren;
        externalChildren.Add(child2);

        Assert.Equal(expected: 2, control.Children.Count);
        Assert.Contains(child1, control.Children);
        Assert.Contains(child2, control.Children);

        externalChildren.Remove(child1);

        Assert.Single(control.Children);
        Assert.Contains(child2, control.Children);
    }

    [Fact]
    public void MultiChildControl_Set_ShouldBeIgnoredWhenPassingCurrentList()
    {
        MockMultiChildControl control = new();
        MockControl child = new();
        control.Children.Add(child);

        EventObserver observer = new();
        control.ChildAdded += observer.OnEvent;

        IList<Control> sameList = control.Children;
        control.Children = sameList;

        Assert.Equal(expected: 0, observer.InvocationCount);
        Assert.Contains(child, control.Children);
    }

    [Fact]
    public void MultiChildControl_Set_ShouldCopyNormalListWithoutBindingToIt()
    {
        MockMultiChildControl control = new();
        MockControl child1 = new();
        MockControl child2 = new();
        MockControl child3 = new();

        List<Control> normalList = [child1, child2];
        control.Children = normalList;
        normalList.Add(child3);

        Assert.Equal(expected: 2, control.Children.Count);
        Assert.Contains(child1, control.Children);
        Assert.Contains(child2, control.Children);
        Assert.DoesNotContain(child3, control.Children);
    }

    [Fact]
    public void MultiChildControl_Add_ShouldReparentChildToNewParent()
    {
        MockMultiChildControl parent1 = new();
        MockMultiChildControl parent2 = new();
        MockControl child = new();

        parent1.Children.Add(child);
        parent2.Children.Add(child);

        Assert.Empty(parent1.Children);
        Assert.Single(parent2.Children);
        Assert.Equal(parent2, child.Parent.GetValue());
    }

    [Fact]
    public void MultiChildControl_Add_ShouldNotHaveAnEffectWhenAddingChildToItsParent()
    {
        MockMultiChildControl control = new();
        MockControl child = new();

        control.Children.Add(child);

        EventObserver observer = new();
        control.ChildAdded += observer.OnEvent;

        control.Children.Add(child);

        Assert.Equal(control, child.Parent.GetValue());
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void MultiChildControl_ShouldThrowOnSortOfBoundList()
    {
        MockMultiChildControl control = new();
        ListSlot<Control> list = [new MockControl(), new MockControl()];
        control.Children = list;

        Assert.Throws<NotSupportedException>(() => list.Sort((_, _) => 0));
    }

    [Fact]
    public void MultiChildControl_ShouldThrowOnMoveInBoundList()
    {
        MockMultiChildControl control = new();
        ListSlot<Control> list = [new MockControl(), new MockControl()];
        control.Children = list;

        Assert.Throws<NotSupportedException>(() => list.Move(oldIndex: 0, newIndex: 1));
    }

    [Fact]
    public void MultiChildControl_ShouldThrowOnReplaceInBoundList()
    {
        MockMultiChildControl control = new();
        ListSlot<Control> list = [new MockControl(), new MockControl()];
        control.Children = list;

        Assert.Throws<NotSupportedException>(() => list[0] = new MockMultiChildControl());
    }
}
