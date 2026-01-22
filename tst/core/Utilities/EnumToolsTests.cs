// <copyright file="EnumToolsTests.cs" company="VoxelGame">
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
