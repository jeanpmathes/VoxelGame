// <copyright file="ChildPresenterTests.cs" company="VoxelGame">
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

[TestSubject(typeof(ChildPresenter))]
public class ChildPresenterTests : VisualTestBase<ChildPresenter>
{
    private readonly MockSingleChildControl parent;

    public ChildPresenterTests() : base(() => new ChildPresenter())
    {
        parent = new MockSingleChildControl
        {
            Template = {Value = ControlTemplate.Create<MockSingleChildControl>(_ => new ChildPresenter())}
        };

        canvas.Child = parent;
    }

    [Fact]
    public void ChildPresenter_ShouldPresentChildOfTemplateOwner()
    {
        parent.Child = CreateControl("Marker");

        Assert.NotNull(FindVisual("Marker"));
    }

    [Fact]
    public void ChildPresenter_ShouldSwapVisualizationWhenChildTemplateChanges()
    {
        MockControl child = CreateControl("OldMarker");

        parent.Child = child;

        child.Template.Value = CreateTemplate("NewMarker");

        Assert.NotNull(FindVisual("NewMarker"));
        Assert.Null(FindVisual("OldMarker"));
    }
}
