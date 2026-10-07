// <copyright file="ReflectionsTests.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities;
using Xunit;

namespace VoxelGame.Toolkit.Tests.Utilities;

[TestSubject(typeof(Reflections))]
public class ReflectionsTests
{
    [Fact]
    public void Reflections_GetTypeHierarchy_ShouldReturnSelfBaseClassesInterfacesAndObject()
    {
        IReadOnlyList<Type> hierarchy = Reflections.GetTypeHierarchy(typeof(DerivedClass));

        Assert.Equal(typeof(DerivedClass), hierarchy[0]);
        Assert.Equal(typeof(BaseClass), hierarchy[1]);
        Assert.Contains(typeof(IFirst), hierarchy);
        Assert.Contains(typeof(ISecond), hierarchy);
        Assert.Equal(typeof(Object), hierarchy[^1]);
    }

    [Fact]
    public void Reflections_GetTypeHierarchy_ShouldReturnOnlyObjectForObject()
    {
        IReadOnlyList<Type> result = Reflections.GetTypeHierarchy(typeof(Object));

        Assert.Single(result);
        Assert.Equal(typeof(Object), result[0]);
    }

    private interface IFirst;

    private interface ISecond;

    private class BaseClass : IFirst;

    private class DerivedClass : BaseClass, ISecond;
}
