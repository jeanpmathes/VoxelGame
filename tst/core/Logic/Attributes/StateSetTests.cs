// <copyright file="StateSetTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
