// <copyright file="StateTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Tests.Logic.Elements;
using VoxelGame.Core.Tests.Utilities.Resources;
using Xunit;

namespace VoxelGame.Core.Tests.Logic.Attributes;

[TestSubject(typeof(State))]
public class StateTests
{
    [Fact]
    public void State_SetGet_Boolean()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Boolean> booleanAttribute = builder.Define("bool").Boolean().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(booleanAttribute, value: true);

        Assert.True(state.Get(booleanAttribute));
    }

    [Fact]
    public void State_GetSetGet_Boolean()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Boolean> booleanAttribute = builder.Define("bool").Boolean().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.False(state.Get(booleanAttribute));
        state.Set(booleanAttribute, value: true);
        Assert.True(state.Get(booleanAttribute));
    }

    [Fact]
    public void State_SetGet_Int32()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Int32> intAttribute = builder.Define("int").Int32(min: 0, max: 4).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(intAttribute, value: 2);

        Assert.Equal(expected: 2, state.Get(intAttribute));
    }

    [Fact]
    public void State_GetSetGet_Int32()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Int32> intAttribute = builder.Define("int").Int32(min: 0, max: 4).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(expected: 0, state.Get(intAttribute));
        state.Set(intAttribute, value: 1);
        Assert.Equal(expected: 1, state.Get(intAttribute));
    }

    [Fact]
    public void State_SetGet_Enum()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<TestState> enumAttribute = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(enumAttribute, TestState.B);

        Assert.Equal(TestState.B, state.Get(enumAttribute));
    }

    [Fact]
    public void State_GetSetGet_Enum()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<TestState> enumAttribute = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(TestState.A, state.Get(enumAttribute));
        state.Set(enumAttribute, TestState.C);
        Assert.Equal(TestState.C, state.Get(enumAttribute));
    }

    [Fact]
    public void State_SetGet_Flags()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<TestStates> flagsAttribute = builder.Define("flags").Flags<TestStates>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(flagsAttribute, TestStates.A | TestStates.B);

        Assert.Equal(TestStates.A | TestStates.B, state.Get(flagsAttribute));
    }

    [Fact]
    public void State_GetSetGet_Flags()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<TestStates> flagsAttribute = builder.Define("flags").Flags<TestStates>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(TestStates.None, state.Get(flagsAttribute));
        state.Set(flagsAttribute, TestStates.B);
        Assert.Equal(TestStates.B, state.Get(flagsAttribute));
    }

    [Fact]
    public void State_SetGet_List()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<TestStruct> listAttribute = builder.Define("list").List([new TestStruct("a"), new TestStruct("b")]).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(listAttribute, new TestStruct("b"));

        Assert.Equal(new TestStruct("b"), state.Get(listAttribute));
    }

    [Fact]
    public void State_GetSetGet_List()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<TestStruct> listAttribute = builder.Define("list").List([new TestStruct("a"), new TestStruct("b")]).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(new TestStruct("a"), state.Get(listAttribute));
        state.Set(listAttribute, new TestStruct("b"));
        Assert.Equal(new TestStruct("b"), state.Get(listAttribute));
    }

    [Fact]
    public void State_SetGet_Nullable()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Int32?> nullableAttribute = builder.Define("nullable").Int32(min: 0, max: 2).NullableAttribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(nullableAttribute, value: 1);

        Assert.Equal(expected: 1, state.Get(nullableAttribute));
        state.Set(nullableAttribute, value: null);
        Assert.Null(state.Get(nullableAttribute));
    }

    [Fact]
    public void State_GetSetGet_Nullable()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Int32?> nullableAttribute = builder.Define("nullable").Int32(min: 0, max: 2).NullableAttribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Null(state.Get(nullableAttribute));
        state.Set(nullableAttribute, value: 1);
        Assert.Equal(expected: 1, state.Get(nullableAttribute));
    }

    [Fact]
    public void State_SetGet_MultipleAttributes()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Boolean> boolAttribute = builder.Define("bool").Boolean().Attribute();
        IAttribute<Int32> intAttribute = builder.Define("int").Int32(min: 0, max: 2).Attribute();
        IAttribute<TestState> enumAttribute = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(enumAttribute, TestState.B);
        state.Set(boolAttribute, value: true);
        state.Set(intAttribute, value: 1);

        Assert.True(state.Get(boolAttribute));
        Assert.Equal(expected: 1, state.Get(intAttribute));
        Assert.Equal(TestState.B, state.Get(enumAttribute));
    }

    [Fact]
    public void State_GetSetGet_MultipleAttributes()
    {
        StateBuilder builder = new(new MockResourceContext());
        IAttribute<Boolean> boolAttribute = builder.Define("bool").Boolean().Attribute();
        IAttribute<Int32> intAttribute = builder.Define("int").Int32(min: 0, max: 2).Attribute();
        IAttribute<TestState> enumAttribute = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.False(state.Get(boolAttribute));
        Assert.Equal(expected: 0, state.Get(intAttribute));
        Assert.Equal(TestState.A, state.Get(enumAttribute));

        state.Set(boolAttribute, value: true);
        state.Set(intAttribute, value: 1);
        state.Set(enumAttribute, TestState.C);

        Assert.True(state.Get(boolAttribute));
        Assert.Equal(expected: 1, state.Get(intAttribute));
        Assert.Equal(TestState.C, state.Get(enumAttribute));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum TestState
    {
        A,
        B,
        C
    }

    [Flags]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum TestStates
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private record struct TestStruct(String Value);
}
