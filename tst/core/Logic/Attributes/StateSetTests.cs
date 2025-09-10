// <copyright file="StateSetTests.cs" company="VoxelGame">
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

[TestSubject(typeof(StateSet))]
public class StateSetTests
{
    [Fact]
    public void StateSet_GenerationDefault_ShouldContainSpecifiedValues()
    {
        StateBuilder builder = new(new Validator(new MockResourceContext()));
        IAttribute<Boolean> boolAttribute = builder.Define("bool").Boolean().Attribute(generationDefault: true);
        IAttribute<TestState> enumAttribute = builder.Define("enum").Enum<TestState>().Attribute(TestState.C);

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State generationDefault = set.GenerationDefault;

        Assert.True(generationDefault.Get(boolAttribute));
        Assert.Equal(TestState.C, generationDefault.Get(enumAttribute));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum TestState
    {
        A,
        B,
        C
    }
}
