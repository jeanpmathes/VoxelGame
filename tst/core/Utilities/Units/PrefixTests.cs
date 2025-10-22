// <copyright file="PrefixTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
