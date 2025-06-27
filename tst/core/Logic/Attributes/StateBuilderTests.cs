// <copyright file="StateBuilderTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.New;
using VoxelGame.Core.Tests.Utilities.Resources;
using Xunit;

namespace VoxelGame.Core.Tests.Logic.Attributes;

[TestSubject(typeof(StateBuilder))]
public class StateBuilderTests
{
    [Fact]
    public void StateBuilder_BooleanAttribute_ShouldCreateTwoStates()
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("bool").Boolean().Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(expected: 2UL, set.Count);
    }

    [Theory]
    [InlineData(0, 2, 2UL)]
    [InlineData(0, 5, 5UL)]
    [InlineData(-3, 3, 6UL)]
    [InlineData(-5, -3, 2UL)]
    public void StateBuilder_Int32Attribute_ShouldCreateExpectedStates(Int32 min, Int32 max, UInt64 expected)
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("int").Int32(min, max).Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(expected, set.Count);
    }

    [Fact]
    public void StateBuilder_EnumAttribute_ShouldCreateStatesForEachValue1()
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("enum").Enum<LargeState>().Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(expected: 3UL, set.Count);
    }

    [Fact]
    public void StateBuilder_EnumAttribute_ShouldCreateStatesForEachValue2()
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("enum").Enum<SmallState>().Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(expected: 2UL, set.Count);
    }

    [Fact]
    public void StateBuilder_FlagsAttribute_ShouldCreateStatesForEachCombination1()
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("flags").Flags<LargeStates>().Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(expected: 8UL, set.Count);
    }

    [Fact]
    public void StateBuilder_FlagsAttribute_ShouldCreateStatesForEachCombination2()
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("flags").Flags<SmallStates>().Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(expected: 4UL, set.Count);
    }

    [Theory]
    [InlineData(1UL)]
    [InlineData(2UL)]
    [InlineData(3UL)]
    public void StateBuilder_ListAttribute_ShouldCreateStatesEqualToListLength(UInt64 length)
    {
        StateBuilder builder = new(new MockResourceContext());
        var elements = new TestStruct[length];
        for (var i = 0; i < (Int32) length; i++) elements[i] = new TestStruct($"v{i}");

        builder.Define("list").List(elements).Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(length, set.Count);
    }

    [Theory]
    [InlineData(2UL)]
    [InlineData(3UL)]
    public void StateBuilder_NullableAttribute_ShouldIncludeNullState(UInt64 underlyingLength)
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("val").Int32(min: 0, (Int32) underlyingLength).NullableAttribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(underlyingLength + 1UL, set.Count);
    }

    [Fact]
    public void StateBuilder_WithMultipleAttributes_ShouldMultiplyStateCounts()
    {
        StateBuilder builder = new(new MockResourceContext());
        builder.Define("bool").Boolean().Attribute();
        builder.Define("int").Int32(min: 0, max: 3).Attribute();
        builder.Define("list").List([new TestStruct("a"), new TestStruct("b")]).Attribute();

        StateSet set = builder.Build(new Block(), setOffset: 0);

        Assert.Equal(2UL * 3UL * 2UL, set.Count);
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
