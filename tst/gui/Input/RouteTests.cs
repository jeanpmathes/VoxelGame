// <copyright file="RouteTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Tests.Visuals;
using Xunit;

namespace VoxelGame.GUI.Tests.Input;

[TestSubject(typeof(Route))]
public class RouteTests
{
    private readonly MockVisual visual = new();

    [Fact]
    public void Route_Empty_ShouldHaveCountOfZero()
    {
        using Route route = Route.Empty;

        Assert.Equal(expected: 0, route.Count);
    }

    [Fact]
    public void Route_Create_WithNull_ShouldBeEmpty()
    {
        using Route route = Route.Create(null);

        Assert.Equal(expected: 0, route.Count);
    }

    [Fact]
    public void Route_Create_WithSingleVisual_ShouldHaveCountOfOne()
    {
        using Route route = Route.Create(visual);

        Assert.Equal(expected: 1, route.Count);
        Assert.Same(visual, route.Root);
        Assert.Same(visual, route.Target);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public void Route_Create_WithDeepHierarchy_ShouldHaveCountOfHierarchyDepth(Int32 depth)
    {
        MockVisual child = visual.CreateDeepChildHierarchy(depth);

        using Route route = Route.Create(child);

        Assert.Equal(depth + 1, route.Count);
        Assert.Same(visual, route.Root);
        Assert.Same(child, route.Target);

        Assert.Same(visual, route.GetFromTop(0));
        Assert.Same(child, route.GetFromTop(depth));

        Assert.Same(child, route.GetFromBottom(0));
        Assert.Same(visual, route.GetFromBottom(depth));
    }

    [Fact]
    public void Route_FindFirstDifferenceFromTop_ShouldReturnLengthForIdenticalRoutes()
    {
        MockVisual child = visual.CreateDeepChildHierarchy(depth: 5, (_, _) => {});

        using Route route1 = Route.Create(child);
        using Route route2 = Route.Create(child);

        Int32 difference = Route.FindFirstDifferenceFromTop(route1, route2);

        Assert.Equal(5 + 1, difference);
    }

    [Fact]
    public void Route_FindFirstDifferenceFromTop_ShouldReturnZeroForRoutesWithDifferentRoots()
    {
        MockVisual root1 = new();
        root1.CreateDeepChildHierarchy(depth: 5);

        MockVisual root2 = new();
        root2.CreateDeepChildHierarchy(depth: 5);

        using Route route1 = Route.Create(root1);
        using Route route2 = Route.Create(root2);

        Int32 difference = Route.FindFirstDifferenceFromTop(route1, route2);

        Assert.Equal(expected: 0, difference);
    }

    [Fact]
    public void Route_FindFirstDifferenceFromTop_ShouldReturnFirstDifferentIndex()
    {
        List<MockVisual> children = [];
        visual.CreateWideChildHierarchy(width: 2, (child, _) => children.Add(child));

        using Route route1 = Route.Create(children[0]);
        using Route route2 = Route.Create(children[1]);

        Int32 difference = Route.FindFirstDifferenceFromTop(route1, route2);

        Assert.Equal(expected: 1, difference);
    }

    [Fact]
    public void Route_FindFirstDifferenceFromTop_ShouldReturnZeroWhenOneRouteIsEmpty()
    {
        using Route route1 = Route.Create(visual);
        using Route route2 = Route.Empty;

        Int32 difference = Route.FindFirstDifferenceFromTop(route1, route2);

        Assert.Equal(expected: 0, difference);
    }
}
