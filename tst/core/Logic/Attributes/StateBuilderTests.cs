// <copyright file="StateBuilderTests.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Tests.Logic.Elements;
using VoxelGame.Core.Tests.Utilities.Resources;
using Xunit;

namespace VoxelGame.Core.Tests.Logic.Attributes;

[TestSubject(typeof(StateBuilder))]
public class StateBuilderTests
{
    private static StateBuilder CreateStateBuilder()
    {
        Validator validator = new(new MockResourceContext());

        validator.SetScope(new MockBlock());

        return new StateBuilder(validator);
    }

    [Fact]
    public void StateBuilder_BooleanAttribute_ShouldCreateTwoStates()
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("bool").Boolean().Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(expected: 2U, set.Count);
    }

    [Theory]
    [InlineData(0, 2, 2U)]
    [InlineData(0, 5, 5U)]
    [InlineData(-3, 3, 6U)]
    [InlineData(-5, -3, 2U)]
    public void StateBuilder_Int32Attribute_ShouldCreateExpectedStates(Int32 min, Int32 max, UInt32 expected)
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("int").Int32(min, max).Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(expected, set.Count);
    }

    [Fact]
    public void StateBuilder_EnumAttribute_ShouldCreateStatesForEachValue1()
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("enum").Enum<LargeState>().Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(expected: 3U, set.Count);
    }

    [Fact]
    public void StateBuilder_EnumAttribute_ShouldCreateStatesForEachValue2()
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("enum").Enum<SmallState>().Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(expected: 2U, set.Count);
    }

    [Fact]
    public void StateBuilder_FlagsAttribute_ShouldCreateStatesForEachCombination1()
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("flags").Flags<LargeStates>().Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(expected: 8U, set.Count);
    }

    [Fact]
    public void StateBuilder_FlagsAttribute_ShouldCreateStatesForEachCombination2()
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("flags").Flags<SmallStates>().Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(expected: 4U, set.Count);
    }

    [Theory]
    [InlineData(1U)]
    [InlineData(2U)]
    [InlineData(3U)]
    public void StateBuilder_ListAttribute_ShouldCreateStatesEqualToListLength(UInt32 length)
    {
        StateBuilder builder = CreateStateBuilder();
        var elements = new TestStruct[length];
        for (var i = 0; i < length; i++) elements[i] = new TestStruct($"v{i}");

        _ = builder.Define("list").List(elements).Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(length, set.Count);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void StateBuilder_NullableAttribute_ShouldIncludeNullState(Int32 underlyingLength)
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("val").Int32(min: 0, underlyingLength).NullableAttribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal((UInt32) (underlyingLength + 1), set.Count);
    }

    [Fact]
    public void StateBuilder_WithMultipleAttributes_ShouldMultiplyStateCounts()
    {
        StateBuilder builder = CreateStateBuilder();
        _ = builder.Define("bool").Boolean().Attribute();
        _ = builder.Define("int").Int32(min: 0, max: 3).Attribute();
        _ = builder.Define("list").List([new TestStruct("a"), new TestStruct("b")]).Attribute();

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        Assert.Equal(2U * 3U * 2U, set.Count);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum SmallState
    {
        A,
        B
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum LargeState
    {
        A,
        B,
        C
    }

    [Flags]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum SmallStates
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1
    }

    [Flags]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum LargeStates
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private record struct TestStruct(String Value);
}
