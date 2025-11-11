// <copyright file="StateTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

[TestSubject(typeof(State))]
public class StateTests
{
    private static StateBuilder CreateStateBuilder()
    {
        Validator validator = new(new MockResourceContext());
        
        validator.SetScope(new MockBlock());
        
        return new StateBuilder(validator);
    }
    
    [Fact]
    public void State_SetGet_Boolean()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Boolean> booleanAttributeData = builder.Define("bool").Boolean().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(booleanAttributeData, value: true);

        Assert.True(state.Get(booleanAttributeData));
    }

    [Fact]
    public void State_GetSetGet_Boolean()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Boolean> booleanAttributeData = builder.Define("bool").Boolean().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.False(state.Get(booleanAttributeData));
        state.Set(booleanAttributeData, value: true);
        Assert.True(state.Get(booleanAttributeData));
    }

    [Fact]
    public void State_SetGet_Int32()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Int32> intAttributeData = builder.Define("int").Int32(min: 0, max: 4).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(intAttributeData, value: 2);

        Assert.Equal(expected: 2, state.Get(intAttributeData));
    }

    [Fact]
    public void State_GetSetGet_Int32()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Int32> intAttributeData = builder.Define("int").Int32(min: 0, max: 4).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(expected: 0, state.Get(intAttributeData));
        state.Set(intAttributeData, value: 1);
        Assert.Equal(expected: 1, state.Get(intAttributeData));
    }

    [Fact]
    public void State_SetGet_Enum()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<TestState> enumAttributeData = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(enumAttributeData, TestState.B);

        Assert.Equal(TestState.B, state.Get(enumAttributeData));
    }

    [Fact]
    public void State_GetSetGet_Enum()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<TestState> enumAttributeData = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(TestState.A, state.Get(enumAttributeData));
        state.Set(enumAttributeData, TestState.C);
        Assert.Equal(TestState.C, state.Get(enumAttributeData));
    }

    [Fact]
    public void State_SetGet_Flags()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<TestStates> flagsAttributeData = builder.Define("flags").Flags<TestStates>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(flagsAttributeData, TestStates.A | TestStates.B);

        Assert.Equal(TestStates.A | TestStates.B, state.Get(flagsAttributeData));
    }

    [Fact]
    public void State_GetSetGet_Flags()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<TestStates> flagsAttributeData = builder.Define("flags").Flags<TestStates>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(TestStates.None, state.Get(flagsAttributeData));
        state.Set(flagsAttributeData, TestStates.B);
        Assert.Equal(TestStates.B, state.Get(flagsAttributeData));
    }

    [Fact]
    public void State_SetGet_List()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<TestStruct> listAttributeData = builder.Define("list").List([new TestStruct("a"), new TestStruct("b")]).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(listAttributeData, new TestStruct("b"));

        Assert.Equal(new TestStruct("b"), state.Get(listAttributeData));
    }

    [Fact]
    public void State_GetSetGet_List()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<TestStruct> listAttributeData = builder.Define("list").List([new TestStruct("a"), new TestStruct("b")]).Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Equal(new TestStruct("a"), state.Get(listAttributeData));
        state.Set(listAttributeData, new TestStruct("b"));
        Assert.Equal(new TestStruct("b"), state.Get(listAttributeData));
    }

    [Fact]
    public void State_SetGet_Nullable()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Int32?> nullableAttributeData = builder.Define("nullable").Int32(min: 0, max: 2).NullableAttribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(nullableAttributeData, value: 1);

        Assert.Equal(expected: 1, state.Get(nullableAttributeData));
        state.Set(nullableAttributeData, value: null);
        Assert.Null(state.Get(nullableAttributeData));
    }

    [Fact]
    public void State_GetSetGet_Nullable()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Int32?> nullableAttributeData = builder.Define("nullable").Int32(min: 0, max: 2).NullableAttribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.Null(state.Get(nullableAttributeData));
        state.Set(nullableAttributeData, value: 1);
        Assert.Equal(expected: 1, state.Get(nullableAttributeData));
    }

    [Fact]
    public void State_SetGet_MultipleAttributes()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Boolean> boolAttributeData = builder.Define("bool").Boolean().Attribute();
        IAttributeData<Int32> intAttributeData = builder.Define("int").Int32(min: 0, max: 2).Attribute();
        IAttributeData<TestState> enumAttributeData = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;
        state.Set(enumAttributeData, TestState.B);
        state.Set(boolAttributeData, value: true);
        state.Set(intAttributeData, value: 1);

        Assert.True(state.Get(boolAttributeData));
        Assert.Equal(expected: 1, state.Get(intAttributeData));
        Assert.Equal(TestState.B, state.Get(enumAttributeData));
    }

    [Fact]
    public void State_GetSetGet_MultipleAttributes()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Boolean> boolAttributeData = builder.Define("bool").Boolean().Attribute();
        IAttributeData<Int32> intAttributeData = builder.Define("int").Int32(min: 0, max: 2).Attribute();
        IAttributeData<TestState> enumAttributeData = builder.Define("enum").Enum<TestState>().Attribute();
        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State state = set.Default;

        Assert.False(state.Get(boolAttributeData));
        Assert.Equal(expected: 0, state.Get(intAttributeData));
        Assert.Equal(TestState.A, state.Get(enumAttributeData));

        state.Set(boolAttributeData, value: true);
        state.Set(intAttributeData, value: 1);
        state.Set(enumAttributeData, TestState.C);

        Assert.True(state.Get(boolAttributeData));
        Assert.Equal(expected: 1, state.Get(intAttributeData));
        Assert.Equal(TestState.C, state.Get(enumAttributeData));
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
