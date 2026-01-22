// <copyright file="DayOfWeekTests.cs" company="VoxelGame">
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
using VoxelGame.Core.Domain.Chrono;
using Xunit;
using DayOfWeek = VoxelGame.Core.Domain.Chrono.DayOfWeek;

namespace VoxelGame.Core.Tests.Domain.Chrono;

[TestSubject(typeof(DayOfWeek))]
public class DayOfWeekTests
{
    [Fact]
    public void DayOfWeek_ToNumber_ShouldBeOneBasedEnumValue()
    {
        foreach (DayOfWeek dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            Int32 expected = (Int32) dayOfWeek + 1;
            Assert.Equal(expected, dayOfWeek.ToNumber());
        }
    }

    [Fact]
    public void DayOfWeek_Next_ShouldAdvanceWithWrap()
    {
        foreach (DayOfWeek dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            var current = (Int32) dayOfWeek;
            Int32 expectedValue = (current + 1) % Calendar.DaysPerWeek;

            var expected = (DayOfWeek) expectedValue;
            Assert.Equal(expected, dayOfWeek.Next());
        }
    }

    [Fact]
    public void DayOfWeek_Previous_ShouldDecrementWithWrap()
    {
        foreach (DayOfWeek dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            var current = (Int32) dayOfWeek;
            Int32 expectedValue = (current + Calendar.DaysPerWeek - 1) % Calendar.DaysPerWeek;

            var expected = (DayOfWeek) expectedValue;
            Assert.Equal(expected, dayOfWeek.Previous());
        }
    }

    [Fact]
    public void DayOfWeek_NextAndPrevious_ShouldBeInverse()
    {
        foreach (DayOfWeek dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            Assert.Equal(dayOfWeek, dayOfWeek.Next().Previous());
            Assert.Equal(dayOfWeek, dayOfWeek.Previous().Next());
        }
    }

    [Fact]
    public void DayOfWeek_Next_ShouldCycleAfterAWeek()
    {
        foreach (DayOfWeek start in Enum.GetValues<DayOfWeek>())
        {
            DayOfWeek dayOfWeek = start;

            for (var index = 0; index < Calendar.DaysPerWeek; index++)
            {
                dayOfWeek = dayOfWeek.Next();
            }

            Assert.Equal(start, dayOfWeek);
        }
    }
}
