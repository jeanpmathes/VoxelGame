// <copyright file="EnumToolsTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities;

[TestSubject(typeof(EnumTools))]
public class EnumToolsTests
{
    [Fact]
    public void EnumTools_IsFlagsEnum_ShouldReturnTrueForFlagsEnum()
    {
        Assert.True(EnumTools.IsFlagsEnum<Values>());
    }

    [Fact]
    public void EnumTools_IsFlagsEnum_ShouldReturnFalseForNonFlagsEnum()
    {
        Assert.False(EnumTools.IsFlagsEnum<Value>());
    }

    [Fact]
    public void EnumTools_CountFlags_ShouldReturnCorrectCountForFlagsEnum()
    {
        Assert.Equal(expected: 3, EnumTools.CountFlags<Values>());
    }

    [Fact]
    public void EnumTools_GetPositions_ShouldReturnCorrectPositionsForFlagsEnum()
    {
        IEnumerable<(String name, Values value)> positions = EnumTools.GetPositions<Values>().ToList();

        Assert.Equal(expected: 3, positions.Count());
        Assert.Contains((nameof(Values.Option1), Values.Option1), positions);
        Assert.Contains((nameof(Values.Option2), Values.Option2), positions);
        Assert.Contains((nameof(Values.Option3), Values.Option3), positions);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Flags]
    private enum Values : Byte
    {
        None = 0,
        Option1 = 1 << 0,
        Option2 = 1 << 1,
        Option3 = 1 << 2
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum Value : Byte
    {
        OptionA,
        OptionB,
        OptionC
    }
}
