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
    private static StateBuilder CreateStateBuilder()
    {
        Validator validator = new(new MockResourceContext());
        
        validator.SetScope(new MockBlock());
        
        return new StateBuilder(validator);
    }
    
    [Fact]
    public void StateSet_GenerationDefault_ShouldContainSpecifiedValues()
    {
        StateBuilder builder = CreateStateBuilder();
        IAttributeData<Boolean> boolAttributeData = builder.Define("bool").Boolean().Attribute(generationDefault: true);
        IAttributeData<TestState> enumAttributeData = builder.Define("enum").Enum<TestState>().Attribute(generationDefault: TestState.C);

        StateSet set = builder.Build(new MockBlock(), setOffset: 0);

        State generationDefault = set.GenerationDefault;

        Assert.True(generationDefault.Get(boolAttributeData));
        Assert.Equal(TestState.C, generationDefault.Get(enumAttributeData));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum TestState
    {
        A,
        B,
        C
    }
}
