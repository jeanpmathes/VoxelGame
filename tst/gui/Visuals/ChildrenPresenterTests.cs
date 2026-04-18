// <copyright file="ChildrenPresenterTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Tests.Controls;
using VoxelGame.GUI.Visuals;
using Xunit;

namespace VoxelGame.GUI.Tests.Visuals;

[TestSubject(typeof(ChildrenPresenter))]
public class ChildrenPresenterTests : VisualTestBase<ChildrenPresenter>
{
    private readonly MockMultiChildControl parent;

    public ChildrenPresenterTests() : base(() => new MockChildrenPresenter())
    {
        parent = new MockMultiChildControl
        {
            Template = {Value = ControlTemplate.Create<MockMultiChildControl>(_ => new MockChildrenPresenter())}
        };

        canvas.Child = parent;
    }

    [Fact]
    public void ChildrenPresenter_ShouldPresentChildrenOfTemplateOwner()
    {
        parent.Children.Add(CreateControl("Marker1"));
        parent.Children.Add(CreateControl("Marker2"));
        parent.Children.Add(CreateControl("Marker3"));

        Assert.NotNull(FindVisual("Marker1"));
        Assert.NotNull(FindVisual("Marker2"));
        Assert.NotNull(FindVisual("Marker3"));
    }

    [Fact]
    public void ChildrenPresenter_ShouldRemoveVisualizationOfRemovedChild()
    {
        parent.Children.Add(CreateControl("Marker1"));
        parent.Children.Add(CreateControl("Marker2"));
        parent.Children.Add(CreateControl("Marker3"));

        parent.Children.Remove(parent.Children[1]);

        Assert.NotNull(FindVisual("Marker1"));
        Assert.Null(FindVisual("Marker2"));
        Assert.NotNull(FindVisual("Marker3"));
    }

    [Fact]
    public void ChildrenPresenter_ShouldSwapVisualizationWhenChildrenTemplateChanges()
    {
        MockControl child1 = CreateControl("OldMarker1");
        MockControl child2 = CreateControl("OldMarker2");
        MockControl child3 = CreateControl("OldMarker3");

        parent.Children.Add(child1);
        parent.Children.Add(child2);
        parent.Children.Add(child3);

        child1.Template.Value = CreateTemplate("NewMarker1");
        child2.Template.Value = CreateTemplate("NewMarker2");
        child3.Template.Value = CreateTemplate("NewMarker3");

        Assert.NotNull(FindVisual("NewMarker1"));
        Assert.NotNull(FindVisual("NewMarker2"));
        Assert.NotNull(FindVisual("NewMarker3"));
    }

    private class MockChildrenPresenter : ChildrenPresenter;
}
