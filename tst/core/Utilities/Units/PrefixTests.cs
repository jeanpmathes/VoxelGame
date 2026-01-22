// <copyright file="PrefixTests.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Definitions;
using VoxelGame.Core.Utilities.Units;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities.Units;

[TestSubject(typeof(Prefix))]
public class PrefixTests
{
    [Fact]
    public void Prefix_FindBest_ShouldReturnClosestPrefixInTrivialCase()
    {
        foreach (Prefix prefix in Prefix.All) Assert.Equal(prefix, Prefix.FindBest(prefix.Factor));
    }

    [Fact]
    public void Prefix_FindBest_ShouldReturnClosestPrefixInExtremeCase()
    {
        Assert.Equal(Prefix.Yotta, Prefix.FindBest(Double.MaxValue));
        Assert.Equal(Prefix.Yocto, Prefix.FindBest(value: 1e-30));
        Assert.Equal(Prefix.Unprefixed, Prefix.FindBest(Double.Epsilon));
    }

    [Fact]
    public void Prefix_FindBest_ShouldReturnClosestPrefix()
    {
        Assert.Equal(Prefix.Mega, Prefix.FindBest(value: 1e6));
        Assert.Equal(Prefix.Kilo, Prefix.FindBest(value: 1e5));
        Assert.Equal(Prefix.Kilo, Prefix.FindBest(value: 1e4));
        Assert.Equal(Prefix.Kilo, Prefix.FindBest(value: 1e3));
        Assert.Equal(Prefix.Unprefixed, Prefix.FindBest(value: 1.0));
    }

    [Fact]
    public void Prefix_FindBest_ShouldOnlyReturnAllowedPrefix()
    {
        Assert.Equal(Prefix.Hecto, Prefix.FindBest(value: 1e6, AllowedPrefixes.Hecto));
        Assert.Equal(Prefix.Unprefixed, Prefix.FindBest(value: 1e6, AllowedPrefixes.Unprefixed));
        Assert.Equal(Prefix.Unprefixed, Prefix.FindBest(value: 1e6, AllowedPrefixes.None));
    }
}
