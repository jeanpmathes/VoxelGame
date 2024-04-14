﻿// <copyright file="PrefixTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Units;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities.Units;

public class PrefixTest
{
    [Fact]
    public void TestFindClosestTrivial()
    {
        foreach (Prefix prefix in Prefix.All) Assert.Equal(prefix, Prefix.FindBest(prefix.Factor));
    }

    [Fact]
    public void TestFindClosestExtremes()
    {
        Assert.Equal(Prefix.Exa, Prefix.FindBest(Double.MaxValue));
        Assert.Equal(Prefix.Atto, Prefix.FindBest(value: 1e-30));
        Assert.Equal(Prefix.None, Prefix.FindBest(Double.Epsilon));
    }

    [Fact]
    public void TestFindClosestDetails()
    {
        Assert.Equal(Prefix.Mega, Prefix.FindBest(value: 1e6));
        Assert.Equal(Prefix.Kilo, Prefix.FindBest(value: 1e5));
        Assert.Equal(Prefix.Kilo, Prefix.FindBest(value: 1e4));
        Assert.Equal(Prefix.Kilo, Prefix.FindBest(value: 1e3));
        Assert.Equal(Prefix.None, Prefix.FindBest(value: 1e2));
    }
}
